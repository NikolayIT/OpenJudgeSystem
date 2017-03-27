namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;
    using OJS.Common;
    using OJS.Common.Models;

    public class CPlusPlusZipFileExecutionStrategy : ExecutionStrategy
    {
        private const string SubmissionName = "UserSubmission";
        private readonly Func<CompilerType, string> getCompilerPathFunc;

        public CPlusPlusZipFileExecutionStrategy(Func<CompilerType, string> getCompilerPath)
        {
            this.getCompilerPathFunc = getCompilerPath;
            this.WorkingDirectory = @"D:\CSharpUnitTestsRunnerTestingFolder";
        }

        public string WorkingDirectory { get; set; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            File.WriteAllText(@"D:\Log.txt", "Enter!");
            var result = new ExecutionResult();

            string submissionDestination =
                $@"{this.WorkingDirectory}\{SubmissionName}";
            File.WriteAllBytes($@"{submissionDestination}", executionContext.FileContent);

            var compilerPath = this.getCompilerPathFunc(executionContext.CompilerType);

            var compilationResult = this.Compile(executionContext.CompilerType, compilerPath, string.Empty, submissionDestination);
            if (!compilationResult.IsCompiledSuccessfully)
            {
                return result;
            }

            File.WriteAllText(@"D:\Log.txt","Success!");
            return result;
        }
    }
}
