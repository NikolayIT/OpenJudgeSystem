namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Executors;

    public class CPlusPlusZipFileExecutionStrategy : ExecutionStrategy
    {
        private const string SubmissionName = "UserSubmission.zip";
        private readonly Func<CompilerType, string> getCompilerPathFunc;

        public CPlusPlusZipFileExecutionStrategy(Func<CompilerType, string> getCompilerPath)
        {
            this.getCompilerPathFunc = getCompilerPath;
            this.WorkingDirectory = DirectoryHelpers.CreateTempDirectory();
        }

        ~CPlusPlusZipFileExecutionStrategy()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.WorkingDirectory,true);
        }

        public string WorkingDirectory { get; set; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            string submissionDestination = $@"{this.WorkingDirectory}\{SubmissionName}";

            File.WriteAllBytes(submissionDestination, executionContext.FileContent);

            var compilerPath = this.getCompilerPathFunc(executionContext.CompilerType);

            var compilationResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                submissionDestination);

            if (!compilationResult.IsCompiledSuccessfully)
            {
                return result;
            }

            result.IsCompiledSuccessfully = true;

           var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(
                    compilationResult.OutputFile,
                    test.Input,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit);

                var testResults = this.ExecuteAndCheckTest(
                    test, 
                    processExecutionResult, 
                    checker,
                    processExecutionResult.ReceivedOutput);

                result.TestResults.Add(testResults);
            }
          
            return result;
        }
    }
}
