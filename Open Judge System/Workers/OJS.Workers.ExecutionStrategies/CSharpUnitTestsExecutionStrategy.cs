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
    using OJS.Workers.Compilers;
    using OJS.Workers.Executors;

    public class CSharpUnitTestsExecutionStrategy : CSharpProjectTestsExecutionStrategy
    {
        private const string TestedCode = "TestedCode.cs";

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

            this.ExtractFilesInWorkingDirectory(userSubmissionContent, this.WorkingDirectory);

            var csProjFilePath = this.GetCsProjFilePath();

            var project = new Project(csProjFilePath);

            this.WriteSetupFixture(project.DirectoryPath);

            this.CorrectProjectReferences(project);
            project.Save(csProjFilePath);
            project.ProjectCollection.UnloadAllProjects();

            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result = this.RunUnitTests(
                this.NUnitConsoleRunnerPath,
                executionContext,
                executor,
                checker,
                result,
                csProjFilePath,
                AdditionalExecutionArguments);

            return result;
        }

        protected override ExecutionResult RunUnitTests(
            string consoleRunnerPath,
            ExecutionContext executionContext,
            IExecutor executor,
            IChecker checker,
            ExecutionResult result,
            string csProjFilePath,
            string additionalExecutionArguments)
        {
            var projectDirectory = Path.GetDirectoryName(csProjFilePath);
            var testedCodePath = $"{projectDirectory}\\{TestedCode}";
            var originalTestsPassed = -1;
            var count = 0;

            var tests = executionContext.Tests.OrderBy(x => x.IsTrialTest).ThenBy(x => x.OrderBy);

            foreach (var test in tests)
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
                arguments.AddRange(additionalExecutionArguments.Split(' '));

                var processExecutionResult = executor.Execute(
                    consoleRunnerPath,
                    string.Empty,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    arguments,
                    null,
                    false,
                    true);

                this.ExtractTestResult(
                    processExecutionResult.ReceivedOutput,
                    out var passedTests,
                    out var totalTests);

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
                    else
                    {
                        message = "Test Passed!";
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

            var compiler = Compiler.CreateCompiler(compilerType);
            var compilerResult = compiler.Compile(compilerPath, submissionFilePath, compilerArguments);
            return compilerResult;
        }

        /// <summary>
        /// Grabs the last match from a match collection
        /// thus ensuring that the tests output is the genuine one,
        /// preventing the user from tampering with it
        /// </summary>
        /// <param name="receivedOutput"></param>
        /// <param name="passedTests"></param>
        /// <param name="totalTests"></param>
        private void ExtractTestResult(string receivedOutput, out int passedTests, out int totalTests)
        {           
            var testResultsRegex = new Regex(TestResultsRegex);
            var res = testResultsRegex.Matches(receivedOutput);
            if (res.Count == 0)
            {
                throw new ArgumentException("The process did not produce any output!");
            }

            totalTests = int.Parse(res[res.Count - 1].Groups[1].Value);
            passedTests = int.Parse(res[res.Count - 1].Groups[2].Value);
        }

        private void CorrectProjectReferences(Project project)
        {
            project.AddItem("Compile", $"{SetupFixtureFileName}{GlobalConstants.CSharpFileExtension}");

            this.EnsureAssemblyNameIsCorrect(project);

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