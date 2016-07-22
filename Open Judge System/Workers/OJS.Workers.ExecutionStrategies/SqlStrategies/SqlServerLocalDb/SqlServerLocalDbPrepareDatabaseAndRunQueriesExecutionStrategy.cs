namespace OJS.Workers.ExecutionStrategies.SqlStrategies.SqlServerLocalDb
{
    public class SqlServerLocalDbPrepareDatabaseAndRunQueriesExecutionStrategy : BaseSqlServerLocalDbExecutionStrategy, IExecutionStrategy
    {
        public ExecutionResult Execute(ExecutionContext executionContext)
        {
            return this.Execute(
                executionContext,
                (connection, test, result) =>
                {
                    this.ExecuteNonQuery(connection, test.Input);
                    var sqlTestResult = this.ExecuteReader(connection, executionContext.Code, executionContext.TimeLimit);
                    this.ProcessSqlResult(sqlTestResult, executionContext, test, result);
                });
        }
    }
}
