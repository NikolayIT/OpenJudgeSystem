namespace OJS.Workers.ExecutionStrategies
{
    using System.Collections.Generic;

    public class DoNothingExecutionStrategy : IExecutionStrategy
    {
        public ExecutionResult Execute(ExecutionContext executionContext)
        {
            return new ExecutionResult
                       {
                           CompilerComment = null,
                           IsCompiledSuccessfully = true,
                           TestResults = new List<TestResult>(),
                       };
        }
    }
}
