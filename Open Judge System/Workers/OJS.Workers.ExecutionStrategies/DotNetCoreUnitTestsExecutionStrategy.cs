namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
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
            var result = new ExecutionResult();

            Directory.CreateDirectory(this.NUnitLiteConsoleAppDirectory);
            Directory.CreateDirectory(this.UserProjectDirectory);
            Directory.CreateDirectory(this.TestedCodeDirectory);

            File.WriteAllText(this.TestedCodeCsProjPath, TestedCodeCsProjTemplate);

            this.ExtractFilesInWorkingDirectory(executionContext.FileContent, this.UserProjectDirectory);

            var nunitLiteConsoleApp = this.CreateNunitLiteConsoleApp(
                new List<string> { this.TestedCodeCsProjPath });

            this.nUnitLiteConsoleAppCsProjTemplate = nunitLiteConsoleApp.csProjTemplate;

            this.MoveUserTestsToNunitLiteConsoleAppFolder();

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
            var testedCodePath = $"{this.TestedCodeDirectory}\\{TestedCode}";
            var originalTestsPassed = -1;
            var count = 0;

            var tests = executionContext.Tests.OrderBy(x => x.IsTrialTest).ThenBy(x => x.OrderBy);
            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);

            foreach (var test in tests)
            {
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

                var arguments = new List<string>
                {
                    compilerResult.OutputFile,
                    additionalExecutionArguments
                };

                var processExecutionResult = executor.Execute(
                    compilerPath,
                    string.Empty,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    arguments,
                    workingDirectory: null,
                    useProcessTime: false,
                    useSystemEncoding: true);

                // Recreate NUnitLite Console App .csproj file, deleted after compilation
                this.CreateNuinitLiteConsoleAppCsProjFile(this.nUnitLiteConsoleAppCsProjTemplate);
            }

            return result;
        }

        private void MoveUserTestsToNunitLiteConsoleAppFolder()
        {
            var userSubmissionFiles = FileHelpers.FindAllFilesMatchingPattern(
                this.UserProjectDirectory, $"*{GlobalConstants.CSharpFileExtension}");

            foreach (var userFile in userSubmissionFiles)
            {
                var userFileInfo = new FileInfo(userFile);
                var destination = $@"{this.NUnitLiteConsoleAppDirectory}\{userFileInfo.Name}";

                if (File.Exists(userFile))
                {
                    File.Move(userFile, destination);
                }
            }
        }
    }
}