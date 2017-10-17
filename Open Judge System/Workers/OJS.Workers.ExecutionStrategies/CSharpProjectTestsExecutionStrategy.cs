namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Evaluation;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Executors;

    public class CSharpProjectTestsExecutionStrategy : ExecutionStrategy
    {
        protected const string SetupFixtureTemplate = @"
        using System;
        using System.IO;
        using NUnit.Framework;


        [SetUpFixture]
        public class SetUpClass
        {
            [OneTimeSetUp]
            public void RedirectConsoleOutputBeforeEveryTest()
            {
                TextWriter writer = new StringWriter();
                Console.SetOut(writer);
            }
        }
";

        protected const string SetupFixtureFileName = "_$SetupFixture";
        protected const string ZippedSubmissionName = "Submission.zip";
        protected const string CompeteTest = "Test";
        protected const string TrialTest = "Test.000";
        protected const string CsProjFileSearchPattern = "*.csproj";
        protected const string NUnitReference =
            "nunit.framework, Version=3.8.0.0, Culture=neutral, PublicKeyToken=2638cd05610744eb, processorArchitecture=MSIL";
        protected const string EntityFrameworkCoreInMemory =
                "Microsoft.EntityFrameworkCore.InMemory, Version=1.1.3.0, Culture=neutral, PublicKeyToken=adb9793829ddae60, processorArchitecture=MSIL";
        protected const string AdditionalExecutionArguments = "--noresult --inprocess";

        // Extracts the number of total and passed tests 
        protected const string TestResultsRegex =
            @"Test Count: (\d+), Passed: (\d+), Failed: (\d+), Warnings: \d+, Inconclusive: \d+, Skipped: \d+";
        // Extracts error/failure messages and the class which threw it       
        protected static readonly string ErrorMessageRegex = $@"(\d+\) (?:Failed|Error)\s:\s(.*)\.(.*)){Environment.NewLine}((?:.*){Environment.NewLine}(?:.*))";

        public CSharpProjectTestsExecutionStrategy(Func<CompilerType, string> getCompilerPathFunc)
        {
            this.GetCompilerPathFunc = getCompilerPathFunc;
            this.TestNames = new List<string>();
            this.TestPaths = new List<string>();
        }

        public CSharpProjectTestsExecutionStrategy(
            string nUnitConsoleRunnerPath,
            Func<CompilerType, string> getCompilerPathFunc)
        {
            if (!File.Exists(nUnitConsoleRunnerPath))
            {
                throw new ArgumentException(
                    $"NUnitConsole not found in: {nUnitConsoleRunnerPath}",
                    nameof(nUnitConsoleRunnerPath));
            }

            this.NUnitConsoleRunnerPath = nUnitConsoleRunnerPath;
            this.GetCompilerPathFunc = getCompilerPathFunc;
            this.TestNames = new List<string>();
            this.TestPaths = new List<string>();
        }

        protected string NUnitConsoleRunnerPath { get; }

        protected Func<CompilerType, string> GetCompilerPathFunc { get; }

        protected string SetupFixturePath { get; set; }

        protected List<string> TestNames { get; }

        protected List<string> TestPaths { get; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();
            var userSubmissionContent = executionContext.FileContent;

            this.ExtractFilesInWorkingDirectory(userSubmissionContent);

            var csProjFilePath = this.GetCsProjFilePath();

            this.ExtractTestNames(executionContext.Tests);

            // Modify Project file
            var project = new Project(csProjFilePath);
            var compileDirectory = project.DirectoryPath;
            this.SetupFixturePath = $"{compileDirectory}\\{SetupFixtureFileName}{GlobalConstants.CSharpFileExtension}";

            this.CorrectProjectReferences(executionContext.Tests, project);
            this.WriteTestFiles(executionContext, compileDirectory);

            this.TestPaths.Add(this.SetupFixturePath);

            // Compiling
            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);
            var compilerResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                csProjFilePath);

            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            result.CompilerComment = compilerResult.CompilerComment;

            if (!compilerResult.IsCompiledSuccessfully)
            {
                return result;
            }

            // Delete tests before execution so the user can't access them
            FileHelpers.DeleteFiles(this.TestPaths.ToArray());

            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result = this.RunUnitTests(executionContext, executor, checker, result, compilerResult.OutputFile);
            return result;
        }

        protected void WriteTestFiles(ExecutionContext executionContext, string compileDirectory)
        {       
            var index = 0;
            foreach (var test in executionContext.Tests)
            {
                var testName = this.TestNames[index++];
                var testedCodePath = $"{compileDirectory}\\{testName}{GlobalConstants.CSharpFileExtension}";
                this.TestPaths.Add(testedCodePath);
                File.WriteAllText(testedCodePath, test.Input);
            }
        }

        protected virtual ExecutionResult RunUnitTests(
            ExecutionContext executionContext,
            IExecutor executor,
            IChecker checker,
            ExecutionResult result,
            string compiledFile)
        {
            var arguments = new List<string> { $"\"{compiledFile}\"" };
            arguments.AddRange(AdditionalExecutionArguments.Split(' '));

            var processExecutionResult = executor.Execute(
                this.NUnitConsoleRunnerPath,
                string.Empty,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                arguments,
                null,
                false,
                true);

            var (totalTestsCount, failedTestsCount) = this.ExtractTotalFailedTestsCount(processExecutionResult.ReceivedOutput);
            var errorsByFiles = this.GetTestErrors(processExecutionResult.ReceivedOutput);

            if (failedTestsCount != errorsByFiles.Count || totalTestsCount != executionContext.Tests.Count())
            {
                throw new ArgumentException("Failing tests not captured properly, please contact an administrator");
            }

            var testIndex = 0;

            foreach (var test in executionContext.Tests)
            {
                var message = "Test Passed!";
                var testFile = this.TestNames[testIndex++];
                if (errorsByFiles.ContainsKey(testFile))
                {
                    message = errorsByFiles[testFile];
                }

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, message);
                result.TestResults.Add(testResult);
            }

            return result;
        }

        protected virtual Dictionary<string, string> GetTestErrors(string receivedOutput)
        {
            var errorsByFiles = new Dictionary<string, string>();
            var errorRegex = new Regex(ErrorMessageRegex);
            var errors = errorRegex.Matches(receivedOutput);

            foreach (Match error in errors)
            {
                var failedAssert = error.Groups[1].Value;
                var cause = error.Groups[4].Value.Replace(Environment.NewLine, string.Empty);
                var fileName = error.Groups[2].Value;
                errorsByFiles.Add(fileName, $"{failedAssert} : {cause}");
            }

            return errorsByFiles;
        }

        protected virtual void CorrectProjectReferences(IEnumerable<TestContext> tests, Project project)
        {
            File.WriteAllText(this.SetupFixturePath, SetupFixtureTemplate);
            project.AddItem("Compile", $"{SetupFixtureFileName}{GlobalConstants.CSharpFileExtension}");

            this.EnsureAssemblyNameIsCorrect(project);

            foreach (var testName in this.TestNames)
            {
                project.AddItem("Compile", $"{testName}{GlobalConstants.CSharpFileExtension}");
            }

            project.SetProperty("OutputType", "Library");

            this.AddProjectReferences(project, NUnitReference, EntityFrameworkCoreInMemory);

            // Check for VSTT just in case, we don't want Assert conflicts
            var vsTestFrameworkReference = project.Items
                .FirstOrDefault(x => x.EvaluatedInclude.Contains("Microsoft.VisualStudio.QualityTools.UnitTestFramework"));
            if (vsTestFrameworkReference != null)
            {
                project.RemoveItem(vsTestFrameworkReference);
            }

            project.Save(project.FullPath);
            project.ProjectCollection.UnloadAllProjects();
        }

        protected void AddProjectReferences(Project project, params string[] references)
        {
            this.RemoveExistingReferences(project, references);
            var referenceMetaData = new Dictionary<string, string>();
            foreach (var reference in references)
            {
                referenceMetaData.Add("SpecificVersion", "False");
                referenceMetaData.Add("Private", "True");
                project.AddItem("Reference", reference, referenceMetaData);
                referenceMetaData.Clear();
            }
        }

        protected virtual void ExtractTestNames(IEnumerable<TestContext> tests)
        {
            foreach (var test in tests)
            {
                string testName = CSharpPreprocessorHelper.GetClassName(test.Input);
                this.TestNames.Add(testName);
            }
        }

        protected void EnsureAssemblyNameIsCorrect(Project project)
        {
            var assemblyNameProperty = project.AllEvaluatedProperties.FirstOrDefault(x => x.Name == "AssemblyName");
            if (assemblyNameProperty == null)
            {
                throw new ArgumentException("Project file does not contain Assembly Name property!");
            }

            var csProjFullpath = project.FullPath;
            var projectName = Path.GetFileNameWithoutExtension(csProjFullpath);
            project.SetProperty("AssemblyName", projectName);
        }

        protected void ExtractFilesInWorkingDirectory(byte[] userSubmissionContent)
        {
            var submissionFilePath = $"{this.WorkingDirectory}\\{ZippedSubmissionName}";
            File.WriteAllBytes(submissionFilePath, userSubmissionContent);
            FileHelpers.RemoveFilesFromZip(submissionFilePath, RemoveMacFolderPattern);
            FileHelpers.UnzipFile(submissionFilePath, this.WorkingDirectory);
            File.Delete(submissionFilePath);
        }

        protected virtual string GetCsProjFilePath() => FileHelpers.FindFileMatchingPattern(
            this.WorkingDirectory,
            CsProjFileSearchPattern,
            f => new FileInfo(f).Length);

        private void RemoveExistingReferences(Project project, string[] references)
        {
            foreach (var reference in references)
            {
                var referenceName = reference.Substring(0, reference.IndexOf(","));
                var existingReference = project.Items.FirstOrDefault(x => x.EvaluatedInclude.Contains(referenceName));
                if (existingReference != null)
                {
                    project.RemoveItem(existingReference);
                }
            }
        }

        private (int totalTestsCount, int failedTestsCount) ExtractTotalFailedTestsCount(string testsOutput)
        {
            var testsSummaryMatcher = new Regex(TestResultsRegex);
            var testsSummaryMatches = testsSummaryMatcher.Matches(testsOutput);
            if (testsSummaryMatches.Count == 0)
            {
                throw new ArgumentException("The process did not produce any output!");
            }

            var failedTestsCount = int.Parse(testsSummaryMatches[testsSummaryMatches.Count - 1].Groups[3].Value);
            var totalTestsCount = int.Parse(testsSummaryMatches[testsSummaryMatches.Count - 1].Groups[1].Value);
            return (totalTestsCount, failedTestsCount);
        }
    }
}