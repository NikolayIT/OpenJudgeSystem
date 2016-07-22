namespace OJS.Workers.ExecutionStrategies.SqlStrategies.SqlServerLocalDb
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;

    public abstract class BaseSqlServerLocalDbExecutionStrategy : BaseSqlExecutionStrategy
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private const string DateTimeOffsetFormat = "yyyy-MM-dd HH:mm:ss.fffffff zzz";
        private const string TimeSpanFormat = "HH:mm:ss.fffffff";

        private static readonly Type DateTimeOffsetType = typeof(DateTimeOffset);

        protected override IDbConnection GetOpenConnection(string databaseName)
        {
            // TODO: Extract as a setting or constructor parameter!
            const string LocalDbBaseConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True;";

            var databaseFilePath = $"C:\\Windows\\Temp\\{databaseName}.mdf";

            using (var connection = new SqlConnection(LocalDbBaseConnectionString))
            {
                connection.Open();

                // TODO: Params?
                this.ExecuteNonQuery(
                    connection,
                    $"CREATE DATABASE [{databaseName}] ON PRIMARY (NAME=N'{databaseName}', FILENAME=N'{databaseFilePath}');");

                // TODO: Extract login and password to settings and constructor parameters!
                // TODO: Params?
                this.ExecuteNonQuery(
                    connection,
                    $@"
IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name=N'{"OJS-Restricted"}')
BEGIN
    CREATE LOGIN [{"OJS-Restricted"}] WITH PASSWORD=N'{"123123"}',
    DEFAULT_DATABASE=[master],
    DEFAULT_LANGUAGE=[us_english],
    CHECK_EXPIRATION=OFF,
    CHECK_POLICY=ON;
END;");

                // TODO: Extract login name to setting and constructor parameter!
                // TODO: Params?
                this.ExecuteNonQuery(
                    connection,
                    $@"
USE [{databaseName}];
CREATE USER [{"OJS-Restricted"}] FOR LOGIN [{"OJS-Restricted"}];
ALTER ROLE [db_owner] ADD MEMBER [{"OJS-Restricted"}];");
            }

            // TODO: Construct with class fields!
            var createdDbConnectionString =
                $"Data Source=(LocalDB)\\MSSQLLocalDB;User Id={"OJS-Restricted"};Password={"123123"};AttachDbFilename={databaseFilePath}";
            var createdDbConnection = new SqlConnection(createdDbConnectionString);
            createdDbConnection.Open();

            return createdDbConnection;
        }

        protected override void DropDatabase(string databaseName)
        {
            // TODO: Extract as a setting and use constructor parameter!
            const string LocalDbBaseConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True;";

            using (var connection = new SqlConnection(LocalDbBaseConnectionString))
            {
                connection.Open();

                this.ExecuteNonQuery(
                    connection,
                    $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}];");
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

                if (fieldType == DateTimeOffsetType)
                {
                    return ((SqlDataReader)dataRecord)
                        .GetDateTimeOffset(index)
                        .ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture);
                }

                if (fieldType == TimeSpanType)
                {
                    return ((SqlDataReader)dataRecord)
                        .GetTimeSpan(index)
                        .ToString(TimeSpanFormat, CultureInfo.InvariantCulture);
                }
            }

            return base.GetDataRecordFieldValue(dataRecord, index);
        }
    }
}
