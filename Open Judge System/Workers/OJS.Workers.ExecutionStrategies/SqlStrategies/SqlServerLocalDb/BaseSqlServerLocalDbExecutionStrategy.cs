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

        private readonly string masterDbConnectionString;
        private readonly string restrictedUserId;
        private readonly string restrictedUserPassword;

        protected BaseSqlServerLocalDbExecutionStrategy(
            string masterDbConnectionString,
            string restrictedUserId,
            string restrictedUserPassword)
        {
            if (string.IsNullOrWhiteSpace(masterDbConnectionString))
            {
                throw new ArgumentException("Invalid master DB connection string!", nameof(masterDbConnectionString));
            }

            if (string.IsNullOrWhiteSpace(restrictedUserId))
            {
                throw new ArgumentException("Invalid restricted user ID!", nameof(restrictedUserId));
            }

            if (string.IsNullOrWhiteSpace(restrictedUserPassword))
            {
                throw new ArgumentException("Invalid restricted user password!", nameof(restrictedUserPassword));
            }

            this.masterDbConnectionString = masterDbConnectionString;
            this.restrictedUserId = restrictedUserId;
            this.restrictedUserPassword = restrictedUserPassword;
        }

        protected override IDbConnection GetOpenConnection(string databaseName)
        {
            var databaseFilePath = $"{this.WorkingDirectory}\\{databaseName}.mdf";

            using (var connection = new SqlConnection(this.masterDbConnectionString))
            {
                connection.Open();

                this.ExecuteNonQuery(
                    connection,
                    $"CREATE DATABASE [{databaseName}] ON PRIMARY (NAME=N'{databaseName}', FILENAME=N'{databaseFilePath}');");

                this.ExecuteNonQuery(
                    connection,
                    $@"
IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name=N'{this.restrictedUserId}')
BEGIN
    CREATE LOGIN [{this.restrictedUserId}] WITH PASSWORD=N'{this.restrictedUserPassword}',
    DEFAULT_DATABASE=[master],
    DEFAULT_LANGUAGE=[us_english],
    CHECK_EXPIRATION=OFF,
    CHECK_POLICY=ON;
END;");

                this.ExecuteNonQuery(
                    connection,
                    $@"
USE [{databaseName}];
CREATE USER [{this.restrictedUserId}] FOR LOGIN [{this.restrictedUserId}];
ALTER ROLE [db_owner] ADD MEMBER [{this.restrictedUserId}];");
            }

            var createdDbConnectionString =
                $"Data Source=(LocalDB)\\MSSQLLocalDB;User Id={this.restrictedUserId};Password={this.restrictedUserPassword};AttachDbFilename={databaseFilePath};Pooling=False;";
            var createdDbConnection = new SqlConnection(createdDbConnectionString);
            createdDbConnection.Open();

            return createdDbConnection;
        }

        protected override void DropDatabase(string databaseName)
        {
            using (var connection = new SqlConnection(this.masterDbConnectionString))
            {
                connection.Open();

                this.ExecuteNonQuery(
                    connection,
                    $@"
IF EXISTS (SELECT name FROM master.sys.databases WHERE name=N'{databaseName}')
BEGIN
    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{databaseName}];
END;");
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
