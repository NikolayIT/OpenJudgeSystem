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
    using Checkers;
    using Common;
    using Executors;

    public class CSharpUnitTestsRunnerExecutionStrategy : ExecutionStrategy
    {
        private const string ZippedSubmissionName = "Submission.zip";
        private const string TestedCode = "TestedCode.cs";
        private const string CsFileExtenstionSearchPattern = "*.cs";
        private const string CsProjFileSearchPattern = "*.csproj";
        private const string ProjFileExtenstion = ".csproj";
        private const string DllFileExtension = ".dll";
        private const string NUnitDirectivesTemplate = @"
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestContext = System.Object;
using TestProperty = NUnit.Framework.PropertyAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using NUnit.Framework;
";

        public CSharpUnitTestsRunnerExecutionStrategy(string nUnitFrameworkPath, string nUnitConsoleRunnerPath)
        {
            this.NUnitFrameworkPath = nUnitFrameworkPath;
            this.NUnitConsoleRunnerPath = nUnitConsoleRunnerPath;
            this.WorkingDirectory = DirectoryHelpers.CreateTempDirectory();
        }

        ~CSharpUnitTestsRunnerExecutionStrategy()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.WorkingDirectory);
        }

        protected string NUnitFrameworkPath { get; set; }

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

            // In order to run unit tests written for the MSTest framework via the NUnit framework we need to
            // switch the standard "using Microsoft.VisualStudio.TestTools.UnitTesting" directives to the NUnit ones 
            // reference: http://www.adamtuliper.com/2009/05/blog-post.html
            this.SwitchUsingDirectivesInCsFiles();

            // Edit References in Project file
            var project = new Project(csProjFilePath);
            this.CorrectProjectReferences(project);
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

        private ExecutionResult RunUnitTests(ExecutionContext executionContext,  IExecutor executor, IChecker checker, ExecutionResult result, string csProjFilePath)
        {
            var compileDirectory = Path.GetDirectoryName(csProjFilePath);

            foreach (var test in executionContext.Tests)
            {
                // Copy the test input into a .cs file
                var testedCodePath = $"{compileDirectory}\\{TestedCode}";
                File.WriteAllText(testedCodePath, test.Input);

                // Compile the project
                var project = new Project(csProjFilePath);
                var didCompile = project.Build();

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
                TestResult testResult = this.EvaluateTestResults(
                    test, 
                    checker,
                    processExecutionResult);

                // Cleanup the .cs with the tested code to prepare for the next test
                File.Delete(testedCodePath);
                result.TestResults.Add(testResult);
            }

            return result;
        }

        private TestResult EvaluateTestResults(TestContext test, IChecker checker, ProcessExecutionResult processExecutionResult)
        {
            TestResult testResult = new TestResult()
            {
                Id = test.Id,
                TimeUsed = (int)processExecutionResult.TimeWorked.TotalMilliseconds,
                MemoryUsed = (int)processExecutionResult.MemoryUsed
            };

            if (processExecutionResult.Type == ProcessExecutionResultType.RunTimeError)
            {
                testResult.ResultType = TestRunResultType.RunTimeError;
                testResult.ExecutionComment = processExecutionResult.ErrorOutput.MaxLength(2048); // Trimming long error texts
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.TimeLimit)
            {
                testResult.ResultType = TestRunResultType.TimeLimit;
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.MemoryLimit)
            {
                testResult.ResultType = TestRunResultType.MemoryLimit;
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.Success)
            {
                string receivedOutput = this.ProcessTestResults(processExecutionResult.ReceivedOutput);
                var checkerResult = checker.Check(test.Input, receivedOutput, test.Output, test.IsTrialTest);
                testResult.ResultType = checkerResult.IsCorrect ? TestRunResultType.CorrectAnswer : TestRunResultType.WrongAnswer;
                testResult.CheckerDetails = checkerResult.CheckerDetails;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(processExecutionResult), "Invalid ProcessExecutionResultType value.");
            }

            return testResult;
        }

        private string ProcessTestResults(string receivedOutput)
        {
            Regex errorRegex =
                new Regex(
                    $@"\d+\)(.*){Environment.NewLine}((?:.|{Environment.NewLine})*?){Environment.NewLine}\s*at \w+(\.[^\.{Environment
                        .NewLine}]*)*?\(\)");
            Regex testResultsRegex =
                new Regex(
                    @"Test Count: (\d+), Passed: (\d+), Failed: (\d+), Warnings: \d+, Inconclusive: \d+, Skipped: \d+");
            var res = testResultsRegex.Match(receivedOutput);
            var errors = errorRegex.Matches(receivedOutput);
            // var totalTests = res.Groups[1].Value; <---- is this really neccessary for UUT?
            var passedTests = res.Groups[2].Value;
            var failedTests = res.Groups[3].Value;
            //List<string> results = new List<string> { $"PassedTests: {passedTests}; FailedTests: {failedTests}" };
            //foreach (Match error in errors)
            //{
            //    var errorMethod = error.Groups[1].Value;
            //    var cause = error.Groups[2].Value.Replace(Environment.NewLine, string.Empty);
            //    results.Add($"{errorMethod} {cause}");
            //}

            return passedTests;
        }

        private void SwitchUsingDirectivesInCsFiles()
        {
            IEnumerable<string> csFilesPaths = Directory.EnumerateFiles(this.WorkingDirectory, CsFileExtenstionSearchPattern, SearchOption.AllDirectories);
            foreach (var path in csFilesPaths)
            {
                string csFile = File.ReadAllText(path);
                csFile = csFile.Replace("using Microsoft.VisualStudio.TestTools.UnitTesting;", NUnitDirectivesTemplate);
                File.WriteAllText(path, csFile);
            }
        }

        private void CorrectProjectReferences(Project project)
        {
            var projectReference = project.GetItems("ProjectReference").FirstOrDefault();
            if (projectReference != null)
            {
                project.RemoveItem(projectReference);
            }

            project.AddItem("Compile", TestedCode);

            Dictionary<string, string> nUnitMetaData = new Dictionary<string, string>();
            nUnitMetaData.Add("HintPath", this.NUnitFrameworkPath);
            nUnitMetaData.Add("Private", "False");
            project.AddItem("Reference", "nunit.framework, Version=3.6.0.0, Culture=neutral, PublicKeyToken=2638cd05610744eb, processorArchitecture=MSIL", nUnitMetaData);
            var vsTestFrameworkReference =
                project.Items.FirstOrDefault(
                    x => x.EvaluatedInclude.Contains("Microsoft.VisualStudio.QualityTools.UnitTestFramework"));
            if (vsTestFrameworkReference != null)
            {
                project.RemoveItem(vsTestFrameworkReference);
            }
        }
    }
}
