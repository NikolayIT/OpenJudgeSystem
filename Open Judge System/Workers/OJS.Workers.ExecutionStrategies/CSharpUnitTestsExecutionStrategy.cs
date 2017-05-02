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
    using OJS.Workers.Compilers;
    using OJS.Workers.Executors;

    public class CSharpUnitTestsExecutionStrategy : CSharpProjectTestsExecutionStrategy
    {
        private const string TestedCode = "TestedCode.cs";

        // Extracts the number of total and passed tests 
        private const string TestResultsRegex =
            @"Test Count: (\d+), Passed: (\d+), Failed: (\d+), Warnings: \d+, Inconclusive: \d+, Skipped: \d+";

        public CSharpUnitTestsExecutionStrategy(
            string nUnitConsoleRunnerPath,
            Func<CompilerType, string> getCompilerPathFunc)
            : base(
                  nUnitConsoleRunnerPath,
                  getCompilerPathFunc)
        {
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();
            var userSubmissionContent = executionContext.FileContent;

            var submissionFilePath = $"{this.WorkingDirectory}\\{ZippedSubmissionName}";
            File.WriteAllBytes(submissionFilePath, userSubmissionContent);
            FileHelpers.UnzipFile(submissionFilePath, this.WorkingDirectory);
            File.Delete(submissionFilePath);

            var csProjFilePath = FileHelpers.FindFirstFileMatchingPattern(
                this.WorkingDirectory,
                CsProjFileSearchPattern);

            var project = new Project(csProjFilePath);
            this.SetupFixturePath = $"{project.DirectoryPath}\\{SetupFixtureFileName}.cs";

            this.CorrectProjectReferences(project);
            project.Save(csProjFilePath);
            project.ProjectCollection.UnloadAllProjects();

            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result = this.RunUnitTests(executionContext, executor, checker, result, csProjFilePath);
            return result;
        }

        protected override ExecutionResult RunUnitTests(
            ExecutionContext executionContext,
            IExecutor executor,
            IChecker checker,
            ExecutionResult result,
            string csProjFilePath)
        {
            var projectDirectory = Path.GetDirectoryName(csProjFilePath);
            var testedCodePath = $"{projectDirectory}\\{TestedCode}";
            var originalTestsPassed = -1;
            var count = 0;

            foreach (var test in executionContext.Tests)
            {
                File.WriteAllText(this.SetupFixturePath, SetupFixtureTemplate);
            
                File.WriteAllText(testedCodePath, test.Input);

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

                // Delete tests before execution so the user can't acces them
                FileHelpers.DeleteFiles(testedCodePath, this.SetupFixturePath);

                var arguments = new List<string> { compilerResult.OutputFile };
                arguments.AddRange(AdditionalExecutionArguments.Split(' '));

                var processExecutionResult = executor.Execute(
                    this.NUnitConsoleRunnerPath,
                    string.Empty,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    arguments);

                var totalTests = 0;
                var passedTests = 0;

                this.ExtractTestResult(processExecutionResult.ReceivedOutput, out passedTests, out totalTests);
                var message = "Test Passed!";

                if (totalTests == 0)
                {
                    message = "No tests found";
                }
                else if (passedTests >= originalTestsPassed)
                {
                    message = "No functionality covering this test!";
                }

                if (count == 0)
                {
                    originalTestsPassed = passedTests;
                    if (totalTests != passedTests)
                    {
                        message = "Not all tests passed on the correct solution.";
                    }
                }

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, message);
                result.TestResults.Add(testResult);
                count++;
            }

            return result;
        }

        protected override CompileResult Compile(
            CompilerType compilerType,
            string compilerPath,
            string compilerArguments,
            string submissionFilePath)
        {
            if (compilerType == CompilerType.None)
            {
                return new CompileResult(true, null) { OutputFile = submissionFilePath };
            }

            if (!File.Exists(compilerPath))
            {
                throw new ArgumentException($"Compiler not found in: {compilerPath}", nameof(compilerPath));
            }

            ICompiler compiler = Compiler.CreateCompiler(compilerType);
            var compilerResult = compiler.Compile(compilerPath, submissionFilePath, compilerArguments);
            return compilerResult;
        }

        private void ExtractTestResult(string receivedOutput, out int passedTests, out int totalTests)
        {
            // Grabbing the last regex match from the nUnit console
            // ensures that the expected output is the genuine one
            // and not some user supplied fake result
            var testResultsRegex = new Regex(TestResultsRegex);
            var res = testResultsRegex.Matches(receivedOutput);
            totalTests = int.Parse(res[res.Count - 1].Groups[1].Value);
            passedTests = int.Parse(res[res.Count - 1].Groups[2].Value);
        }

        private void CorrectProjectReferences(Project project)
        {
            project.AddItem("Compile", $"{SetupFixtureFileName}.cs");

            // Remove the first Project Reference (this should be the reference to the tested project)
            var projectReference = project.GetItems("ProjectReference").FirstOrDefault();
            if (projectReference != null)
            {
                project.RemoveItem(projectReference);
            }

            project.AddItem("Compile", TestedCode);
            project.SetProperty("OutputType", "Library");

            var nUnitPrevReference = project.Items.FirstOrDefault(x => x.EvaluatedInclude.Contains("nunit.framework"));
            if (nUnitPrevReference != null)
            {
                project.RemoveItem(nUnitPrevReference);
            }

            // Add our NUnit Reference, if private is false, the .dll will not be copied and the tests will not run
            var nUnitMetaData = new Dictionary<string, string>();
            nUnitMetaData.Add("SpecificVersion", "False");
            nUnitMetaData.Add("Private", "True");
            project.AddItem("Reference", NUnitReference, nUnitMetaData);

            // If we use NUnit we don't really need the VSTT, it will save us copying of the .dll
            var vsTestFrameworkReference = project.Items
                .FirstOrDefault(x =>
                    x.EvaluatedInclude.Contains("Microsoft.VisualStudio.QualityTools.UnitTestFramework"));

            if (vsTestFrameworkReference != null)
            {
                project.RemoveItem(vsTestFrameworkReference);
            }
        }
    }
}
