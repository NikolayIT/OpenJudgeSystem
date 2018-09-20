namespace OJS.Workers.ExecutionStrategies
{
    using System.Collections.Generic;

    using OJS.Workers.Common.Helpers;

    public class DoNothingExecutionStrategy : IExecutionStrategy
    {
        protected string WorkingDirectory { get; set; }

        public ExecutionResult SafeExecute(ExecutionContext executionContext)
        {
            this.WorkingDirectory = DirectoryHelpers.CreateTempDirectoryForExecutionStrategy();
            try
            {
                return this.Execute(executionContext);
            }
            finally
            {
                DirectoryHelpers.SafeDeleteDirectory(this.WorkingDirectory, true);
            }
        }

        public ExecutionResult Execute(ExecutionContext executionContext) =>
            new ExecutionResult
            {
                CompilerComment = null,
                IsCompiledSuccessfully = true,
                TestResults = new List<TestResult>()
            };
    }
}
