namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.ExecutionStrategies.Helpers.UnitTestStrategies;
    using OJS.Workers.Executors;

    public class DotNetCoreUnitTestsExecutionStrategy : DotNetCoreProjectTestsExecutionStrategy
    {
        private const string TestedCode = "TestedCode.cs";
        private const string TestedCodeFolderName = "TestedCodeProject";
        private const string TestedCodeCsProjTemplate = @"
            <Project Sdk=""Microsoft.NET.Sdk\"">
                <PropertyGroup>
                    <TargetFramework>netcoreapp2.0</TargetFramework>
                </PropertyGroup>
            </Project>";

        private string nUnitLiteConsoleAppCsProjTemplate;

        public DotNetCoreUnitTestsExecutionStrategy(
            Func<CompilerType, string> getCompilerPathFunc,
            int baseTimeUsed,
            int baseMemoryUsed)
                : base(getCompilerPathFunc, baseTimeUsed, baseMemoryUsed)
        {
        }

        private string TestedCodeDirectory => Path.Combine(this.WorkingDirectory, TestedCodeFolderName);

        private string TestedCodeCsProjPath =>
            Path.Combine(this.TestedCodeDirectory, TestedCodeFolderName + CsProjFileExtention);

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            Directory.CreateDirectory(this.NUnitLiteConsoleAppDirectory);
            Directory.CreateDirectory(this.TestedCodeDirectory);

            var result = new ExecutionResult();

            var userSubmission = executionContext.FileContent;

            this.ExtractFilesInWorkingDirectory(userSubmission, this.NUnitLiteConsoleAppDirectory);

            // Create .csproj for the tests to reference it from the NUnitLite console app
            File.WriteAllText(this.TestedCodeCsProjPath, TestedCodeCsProjTemplate);

            var nunitLiteConsoleApp = this.CreateNunitLiteConsoleApp(new List<string> { this.TestedCodeCsProjPath });

            this.nUnitLiteConsoleAppCsProjTemplate = nunitLiteConsoleApp.csProjTemplate;

            var executor = new RestrictedProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result = this.RunUnitTests(
                nunitLiteConsoleApp.csProjPath,
                executionContext,
                executor,
                checker,
                result,
                this.TestedCodeCsProjPath,
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
            var additionalExecutionArgumentsArray = additionalExecutionArguments
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);

            var testedCodePath = $"{this.TestedCodeDirectory}\\{TestedCode}";
            var originalTestsPassed = -1;

            var tests = executionContext.Tests.OrderBy(x => x.IsTrialTest).ThenBy(x => x.OrderBy).ToList();

            for (var i = 0;  i < tests.Count; i++)
            {
                var test = tests[i];

                this.SaveSetupFixture(this.NUnitLiteConsoleAppDirectory);

                File.WriteAllText(testedCodePath, test.Input);

                // Compiling
                var compilerResult = this.Compile(
                    executionContext.CompilerType,
                    compilerPath,
                    executionContext.AdditionalCompilerArguments,
                    consoleRunnerPath);

                result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
                result.CompilerComment = compilerResult.CompilerComment;

                if (!compilerResult.IsCompiledSuccessfully)
                {
                    return result;
                }

                // Delete tests before execution so the user can't acces them
                FileHelpers.DeleteFiles(testedCodePath, this.SetupFixturePath);

                var arguments = new List<string> { compilerResult.OutputFile };
                arguments.AddRange(additionalExecutionArgumentsArray);

                var processExecutionResult = executor.Execute(
                    compilerPath,
                    string.Empty,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    arguments,
                    workingDirectory: null,
                    useProcessTime: false,
                    useSystemEncoding: true);

                var processExecutionTestResult = UnitTestStrategiesHelper.GetTestResult(
                    processExecutionResult.ReceivedOutput,
                    TestResultsRegex,
                    originalTestsPassed,
                    i == 0);

                var message = processExecutionTestResult.message;
                originalTestsPassed = processExecutionTestResult.originalTestsPassed;

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, message);
                result.TestResults.Add(testResult);

                if (i < tests.Count - 1)
                {
                    // Recreate NUnitLite Console App .csproj file, deleted after compilation, to compile again
                    this.CreateNuinitLiteConsoleAppCsProjFile(this.nUnitLiteConsoleAppCsProjTemplate);
                }
            }

            return result;
        }
    }
}