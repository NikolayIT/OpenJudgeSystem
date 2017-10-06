namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Executors;

    public class DotNetCoreProjectExecutionStrategy : CSharpProjectTestsExecutionStrategy
    {
        protected new const string AdditionalExecutionArguments = "--no-build --no-restore";

        public DotNetCoreProjectExecutionStrategy(Func<CompilerType, string> getCompilerPathFunc)
            : base(getCompilerPathFunc)
        {
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            var userSubmissionContent = executionContext.FileContent;

            var submissionFilePath = $"{this.WorkingDirectory}\\{ZippedSubmissionName}";
            File.WriteAllBytes(submissionFilePath, userSubmissionContent);
            FileHelpers.RemoveFilesFromZip(submissionFilePath, RemoveMacFolderPattern);
            FileHelpers.UnzipFile(submissionFilePath, this.WorkingDirectory);

            File.Delete(submissionFilePath);

            var csProjFilePath = FileHelpers.FindFileMatchingPattern(
                this.WorkingDirectory,
                CsProjFileSearchPattern,
                f => new FileInfo(f).Length);

            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);         

            var compilerResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                csProjFilePath);

            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;

            if (!result.IsCompiledSuccessfully)
            {
                return result;
            }
           
            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            List<string> arguments = new List<string>();
            string path =
                @"C:\SideAndTestProjects\DotNetCoreTestRunnerTestProject\CoreTestingApp\bin\Debug\netcoreapp1.1\CoreTestingApp.dll";

            arguments.Add(path); 
            //arguments.Add(AdditionalExecutionArguments);
            arguments.Add("--noresult");

            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(
                    compilerPath,
                    string.Empty,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    arguments,
                    this.WorkingDirectory);

                var testResult = this.ExecuteAndCheckTest(
                    test,
                    processExecutionResult,
                    checker,
                    processExecutionResult.ReceivedOutput);

                result.TestResults.Add(testResult);
            }

            return result;
        }
    }
}
