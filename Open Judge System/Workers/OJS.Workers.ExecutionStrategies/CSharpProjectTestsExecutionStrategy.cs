namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Microsoft.Build.Evaluation;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    public class CSharpProjectTestsExecutionStrategy : ExecutionStrategy
    {
        private const string ZippedSubmissionName = "Submission.zip";
        private const string CompeteTest = "Test";
        private const string TrialTest = "Test.000";
        private const string CsProjFileSearchPattern = "*.csproj";
        private const string ProjFileExtenstion = ".csproj";
        private const string DllFileExtension = ".dll";

        private readonly List<string> testNames;

        public CSharpProjectTestsExecutionStrategy(string nUnitConsoleRunnerPath)
        {
            this.NUnitConsoleRunnerPath = nUnitConsoleRunnerPath;
            this.WorkingDirectory = DirectoryHelpers.CreateTempDirectory();
            this.testNames = new List<string>();
        }

        ~CSharpProjectTestsExecutionStrategy()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.WorkingDirectory, true);
        }

        protected string NUnitConsoleRunnerPath { get; set; }

        protected string WorkingDirectory { get; set; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            ExecutionResult result = new ExecutionResult();
            byte[] userSubmissionContent = executionContext.FileContent;

            var submissionFilePath = $"{this.WorkingDirectory}\\{ZippedSubmissionName}";
            File.WriteAllBytes(submissionFilePath, userSubmissionContent);
            FileHelpers.UnzipFile(submissionFilePath, this.WorkingDirectory);
            File.Delete(submissionFilePath);

            string csProjFilePath = FileHelpers.FindFirstFileMatchingPattern(
                this.WorkingDirectory,
                CsProjFileSearchPattern);

            this.ExtractTestNames(executionContext.Tests);

            // Edit References in Project file
            var project = new Project(csProjFilePath);
            this.CorrectProjectReferences(executionContext.Tests, project);
            project.Save(csProjFilePath);
            project.ProjectCollection.UnloadAllProjects();

            // Initially set isCompiledSucessfully to true
            result.IsCompiledSuccessfully = true;

            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result = this.RunUnitTests(executionContext, executor, checker, result, csProjFilePath);
            return result;
        }

        private ExecutionResult RunUnitTests(ExecutionContext executionContext, IExecutor executor, IChecker checker, ExecutionResult result, string csProjFilePath)
        {
            var compileDirectory = Path.GetDirectoryName(csProjFilePath);
            int index = 0;

            foreach (var test in executionContext.Tests)
            {
                // Copy the test inputs into .cs files
                string testName = this.testNames[index++];
                var testedCodePath = $"{compileDirectory}\\{testName}.cs";
                File.WriteAllText(testedCodePath, test.Input);
            }

            // Compile the project
            var project = new Project(csProjFilePath);
            var didCompile = project.Build();
            project.ProjectCollection.UnloadAllProjects();

            // If a test does not compile, set isCompiledSuccessfully to false and break execution
            if (!didCompile)
            {
                result.IsCompiledSuccessfully = false;
                return result;
            }

            var fileName = Path.GetFileName(csProjFilePath);
            fileName = fileName.Replace(ProjFileExtenstion, DllFileExtension);
            var dllPath = FileHelpers.FindFirstFileMatchingPattern(this.WorkingDirectory, fileName);
            var arguments = new List<string> { dllPath };
            arguments.AddRange(executionContext.AdditionalCompilerArguments.Split(' '));

            // Run unit tests on the resulting .dll
            var processExecutionResult = executor.Execute(
                this.NUnitConsoleRunnerPath,
                string.Empty,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                arguments);

            var errorsByFiles = this.GetTestErrors(processExecutionResult.ReceivedOutput);
            var testIndex = 0;

            foreach (var test in executionContext.Tests)
            {
                // Construct and figure out what the Test results are
                TestResult testResult = new TestResult()
                {
                    Id = test.Id,
                    TimeUsed = (int)processExecutionResult.TimeWorked.TotalMilliseconds,
                    MemoryUsed = (int)processExecutionResult.MemoryUsed
                };

                switch (processExecutionResult.Type)
                {
                    case ProcessExecutionResultType.RunTimeError:
                        testResult.ResultType = TestRunResultType.RunTimeError;
                        testResult.ExecutionComment = processExecutionResult.ErrorOutput.MaxLength(2048); // Trimming long error texts
                        break;
                    case ProcessExecutionResultType.TimeLimit:
                        testResult.ResultType = TestRunResultType.TimeLimit;
                        break;
                    case ProcessExecutionResultType.MemoryLimit:
                        testResult.ResultType = TestRunResultType.MemoryLimit;
                        break;
                    case ProcessExecutionResultType.Success:
                        string message = "Test Passed!";
                        string testFile = this.testNames[testIndex++];
                        if (errorsByFiles.ContainsKey(testFile))
                        {
                            message = errorsByFiles[testFile];
                        }

                        var checkerResult = checker.Check(test.Input, message, test.Output, test.IsTrialTest);
                        testResult.ResultType = checkerResult.IsCorrect ? TestRunResultType.CorrectAnswer : TestRunResultType.WrongAnswer;
                        testResult.CheckerDetails = checkerResult.CheckerDetails;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(processExecutionResult), "Invalid ProcessExecutionResultType value.");
                }

                result.TestResults.Add(testResult);
            }         

            return result;
        }

        private Dictionary<string, string> GetTestErrors(string receivedOutput)
        {
            var errorsByFiles = new Dictionary<string, string>();
            var errorRegex =
     new Regex($@"\d+\) (.*){Environment.NewLine}((?:.+{Environment.NewLine})*?)\s*at (?:[^(){Environment.NewLine}]+?)\(\) in \w:\\(?:[^\\{Environment.NewLine}]+\\)*(.+).cs");

            var errors = errorRegex.Matches(receivedOutput);

            foreach (Match error in errors)
            {
                var errorMethod = error.Groups[1].Value;
                var cause = error.Groups[2].Value.Replace(Environment.NewLine, string.Empty);
                var fileName = error.Groups[3].Value;
                errorsByFiles.Add(fileName, $"{errorMethod} {cause}");
            }

            return errorsByFiles;
        }

        private void CorrectProjectReferences(IEnumerable<TestContext> tests, Project project)
        {
            foreach (var testName in this.testNames)
            {
                project.AddItem("Compile", $"{testName}.cs");
            }

            project.SetProperty("OutputType", "Library");

            // Remove previous NUnit reference (the path is probably pointing to the users package folder)
            var nUnitPrevReference = project.Items.FirstOrDefault(x => x.EvaluatedInclude.Contains("nunit.framework"));
            if (nUnitPrevReference != null)
            {
                project.RemoveItem(nUnitPrevReference);
            }

            // Add our NUnit Reference, if private is false, the .dll will not be copied and the tests will not run
            var nUnitMetaData = new Dictionary<string, string>();
            nUnitMetaData.Add("SpecificVersion", "False");
            nUnitMetaData.Add("Private", "True");
            project.AddItem("Reference", "nunit.framework, Version=3.6.0.0, Culture=neutral, PublicKeyToken=2638cd05610744eb, processorArchitecture=MSIL", nUnitMetaData);

            // Check for VSTT just in case, we dont want Assert conflicts
            var vsTestFrameworkReference =
                project.Items.FirstOrDefault(
                    x => x.EvaluatedInclude.Contains("Microsoft.VisualStudio.QualityTools.UnitTestFramework"));
            if (vsTestFrameworkReference != null)
            {
                project.RemoveItem(vsTestFrameworkReference);
            }
        }

        private void ExtractTestNames(IEnumerable<TestContext> tests)
        {
            int trialTests = 1;
            int competeTests = 1;

            foreach (var test in tests)
            {
                if (test.IsTrialTest)
                {
                    string testNumber = trialTests < 10 ? $"00{trialTests}" : $"0{trialTests}";
                    this.testNames.Add($"{TrialTest}.{testNumber}");
                    trialTests++;
                }
                else
                {
                    string testNumber = competeTests < 10 ? $"00{competeTests}" : $"0{competeTests}";
                    this.testNames.Add($"{CompeteTest}.{testNumber}");
                    competeTests++;
                }
            }
        }
    }
}
