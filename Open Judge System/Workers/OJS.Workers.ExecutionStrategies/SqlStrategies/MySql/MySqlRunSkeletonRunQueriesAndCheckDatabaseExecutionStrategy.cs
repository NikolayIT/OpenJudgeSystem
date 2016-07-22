namespace OJS.Workers.ExecutionStrategies.SqlStrategies.MySql
{
    public class MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy : BaseMySqlExecutionStrategy, IExecutionStrategy
    {
        public MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy(
            string sysDbConnectionString,
            string restrictedUserId,
            string restrictedUserPassword)
            : base(sysDbConnectionString, restrictedUserId, restrictedUserPassword)
        {
        }

        public ExecutionResult Execute(ExecutionContext executionContext)
        {
            return this.Execute(
                executionContext,
                (connection, test, result) =>
                {
                    this.ExecuteNonQuery(connection, executionContext.TaskSkeletonAsString);
                    this.ExecuteNonQuery(connection, executionContext.Code, executionContext.TimeLimit);
                    var sqlTestResult = this.ExecuteReader(connection, test.Input);
                    this.ProcessSqlResult(sqlTestResult, executionContext, test, result);
                });
        }
    }
}
