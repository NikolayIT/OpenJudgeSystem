namespace SqlTestingPoC
{
    using System;

    using OJS.Workers.ExecutionStrategies;

    public class SqlRunUserQueryAndCheckDatabaseExecutionStrategy : IExecutionStrategy
    {
        public ExecutionResult Execute(ExecutionContext executionContext)
        {
            // User code = user queries - prepare database
            // Test input = query to execute
            // Test output = expected result from the query
            throw new NotImplementedException();
        }
    }
}
