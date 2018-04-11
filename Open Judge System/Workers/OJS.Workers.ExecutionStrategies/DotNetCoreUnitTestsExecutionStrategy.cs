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
    using OJS.Workers.Executors;

    public class DotNetCoreUnitTestsExecutionStrategy : DotNetCoreProjectTestsExecutionStrategy
    {
        private const string TestedCode = "TestedCode.cs";

        public DotNetCoreUnitTestsExecutionStrategy(
            Func<CompilerType, string> getCompilerPathFunc,
            int baseTimeUsed,
            int baseMemoryUsed)
                : base(getCompilerPathFunc, baseTimeUsed, baseMemoryUsed)
        {
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            Directory.CreateDirectory(this.NUnitLiteConsoleAppDirectory);
            Directory.CreateDirectory(this.UserProjectDirectory);

            var result = new ExecutionResult();

            var userSubmission = executionContext.FileContent;

            this.ExtractFilesInWorkingDirectory(userSubmission, this.UserProjectDirectory);
            this.SaveSetupFixture(this.NUnitLiteConsoleAppDirectory);

            var userCsProjPaths = new List<string> { this.GetCsProjFilePath() };

            var nunitLiteConsoleAppCsProjPath = this.CreateNunitLiteConsoleApp(userCsProjPaths);

            var executor = new RestrictedProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result = this.RunUnitTests(
                nunitLiteConsoleAppCsProjPath,
                executionContext,
                executor,
                checker,
                result,
                userCsProjPaths.First(),
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
            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);

            foreach (var test in tests)
            {
                File.WriteAllText(this.SetupFixturePath, SetupFixtureTemplate);

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

                //// Delete tests before execution so the user can't acces them
                FileHelpers.DeleteFiles(testedCodePath, this.SetupFixturePath);

                var arguments = new List<string>
                {
                    "test",
                    csProjFilePath,
                    "--no-build",
                    "--no-restore",
                    "--output",
                    Path.GetDirectoryName(compilerResult.OutputFile)
                };

                var processExecutionResult = executor.Execute(
                    consoleRunnerPath,
                    string.Empty,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    arguments,
                    null,
                    false,
                    true);
            }

            return result;
        }
    }
}