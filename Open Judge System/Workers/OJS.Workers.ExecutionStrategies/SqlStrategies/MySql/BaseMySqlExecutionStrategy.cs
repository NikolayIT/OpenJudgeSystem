namespace OJS.Workers.ExecutionStrategies.SqlStrategies.MySql
{
    using System.Data;
    using System.Globalization;

    using global::MySql.Data.MySqlClient;

    public abstract class BaseMySqlExecutionStrategy : BaseSqlExecutionStrategy
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private const string TimeSpanFormat = "HH:mm:ss";

        protected override IDbConnection GetOpenConnection(string databaseName)
        {
            // TODO: Extract as a setting or constructor parameters!
            // TODO: Construct with class fields!
            const string MySqlMasterDbConnectionString = "Server=localhost;Database=sys;UID=root;Password=root";

            using (var connection = new MySqlConnection(MySqlMasterDbConnectionString))
            {
                connection.Open();

                // TODO: Params?
                this.ExecuteNonQuery(connection, $"CREATE DATABASE `{databaseName}`;");

                // TODO: Params?
                this.ExecuteNonQuery(
                    connection,
                    $@"
CREATE USER IF NOT EXISTS '{"OJS-Restricted"}'@'{"localhost"}';
SET PASSWORD FOR '{"OJS-Restricted"}'@'{"localhost"}'='{"123123"}'");

                // TODO: Params?
                this.ExecuteNonQuery(
                    connection,
                    $@"
GRANT ALL PRIVILEGES ON `{databaseName}`.* TO '{"OJS-Restricted"}'@'{"localhost"}';
FLUSH PRIVILEGES;");
            }

            // TODO: Construct with class fields!
            var createdDbConnectionString =
                $"Server=localhost;Database={databaseName};UID={"OJS-Restricted"};Password={"123123"}";
            var createdDbConnection = new MySqlConnection(createdDbConnectionString);
            createdDbConnection.Open();

            return createdDbConnection;
        }

        protected override void DropDatabase(string databaseName)
        {
            // TODO: Extract as a setting or constructor parameters!
            // TODO: Construct with class fields!
            const string MySqlMasterDbConnectionString = "Server=localhost;Database=sys;UID=root;Password=root";

            using (var connection = new MySqlConnection(MySqlMasterDbConnectionString))
            {
                connection.Open();

                this.ExecuteNonQuery(connection, $"SET FOREIGN_KEY_CHECKS = 0; DROP DATABASE `{databaseName}`;");
            }
        }

        protected override string GetDataRecordFieldValue(IDataRecord dataRecord, int index)
        {
            if (!dataRecord.IsDBNull(index))
            {
                var fieldType = dataRecord.GetFieldType(index);

                if (fieldType == DateTimeType)
                {
                    return dataRecord.GetDateTime(index).ToString(DateTimeFormat, CultureInfo.InvariantCulture);
                }

                if (fieldType == TimeSpanType)
                {
                    return ((MySqlDataReader)dataRecord)
                        .GetTimeSpan(index)
                        .ToString(TimeSpanFormat, CultureInfo.InvariantCulture);
                }
            }

            return base.GetDataRecordFieldValue(dataRecord, index);
        }
    }
}
