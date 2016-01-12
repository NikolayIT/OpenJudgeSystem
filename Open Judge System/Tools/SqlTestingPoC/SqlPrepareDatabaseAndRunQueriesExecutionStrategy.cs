namespace SqlTestingPoC
{
    using System;

    using OJS.Workers.ExecutionStrategies;

    public class SqlPrepareDatabaseAndRunUserQueryExecutionStrategy : IExecutionStrategy
    {
        public ExecutionResult Execute(ExecutionContext executionContext)
        {
            // Test input = prepare database (first queries)
            // User code = query to execute
            // Test output = expected result from user query
            throw new NotImplementedException();
        }
    }
}
