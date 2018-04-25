namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.Build.Evaluation;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Compilers;
    using OJS.Workers.ExecutionStrategies.Extensions;
    using OJS.Workers.ExecutionStrategies.Helpers;
    using OJS.Workers.Executors;

    public class CSharpUnitTestsExecutionStrategy : CSharpProjectTestsExecutionStrategy
    {
        private const string NUnitFrameworkPackageName = "nunit.framework";

        public CSharpUnitTestsExecutionStrategy(
            string nUnitConsoleRunnerPath,
            Func<CompilerType, string> getCompilerPathFunc,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(nUnitConsoleRunnerPath, getCompilerPathFunc, baseTimeUsed, baseMemoryUsed)
        {
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            var userSubmissionContent = executionContext.FileContent;

            this.ExtractFilesInWorkingDirectory(userSubmissionContent, this.WorkingDirectory);

            var csProjFilePath = this.GetCsProjFilePath();

            var project = new Project(csProjFilePath);

            this.SaveSetupFixture(project.DirectoryPath);

            this.CorrectProjectReferences(project);

            var executor = new RestrictedProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);
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
            var testedCodePath = $"{projectDirectory}\\{UnitTestStrategiesHelper.TestedCodeFileName}";
            var originalTestsPassed = -1;
            var count = 0;

            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);

            var tests = executionContext.Tests.OrderBy(x => x.IsTrialTest).ThenBy(x => x.OrderBy);

            foreach (var test in tests)
            {
                File.WriteAllText(this.SetupFixturePath, SetupFixtureTemplate);
            
                File.WriteAllText(testedCodePath, test.Input);

                // Compiling
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

                var processExecutionTestResult = UnitTestStrategiesHelper.GetTestResult(
                    processExecutionResult.ReceivedOutput,
                    TestResultsRegex,
                    originalTestsPassed,
                    count == 0);

                var message = processExecutionTestResult.message;
                originalTestsPassed = processExecutionTestResult.originalTestsPassed;

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

        private void CorrectProjectReferences(Project project)
        {
            project.AddItem("Compile", $"{SetupFixtureFileName}{GlobalConstants.CSharpFileExtension}");

            project.EnsureAssemblyNameIsCorrect();

            // Remove the first Project Reference (this should be the reference to the tested project)
            var projectReference = project.GetItems("ProjectReference").FirstOrDefault();
            if (projectReference != null)
            {
                project.RemoveItem(projectReference);
            }

            project.AddItem("Compile", UnitTestStrategiesHelper.TestedCodeFileName);
            project.SetProperty("OutputType", "Library");

            project.RemoveItemByName(NUnitFrameworkPackageName);

            // Add our NUnit Reference
            project.AddReferences(NUnitReference);

            // If we use NUnit we don't really need the VSTT, it will save us copying of the .dll
            project.RemoveItemByName(VsttPackageName);

            project.Save(project.FullPath);
            project.ProjectCollection.UnloadAllProjects();

            project.RemoveNuGetPackageImportsTarget();
        }
    }
}