namespace OJS.Workers.ExecutionStrategies.SqlStrategies.MySql
{
    using System;
    using System.Data;
    using System.Globalization;

    using global::MySql.Data.MySqlClient;

    public abstract class BaseMySqlExecutionStrategy : BaseSqlExecutionStrategy
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private const string TimeSpanFormat = "HH:mm:ss";

        private readonly string sysDbConnectionString;
        private readonly string restrictedUserId;
        private readonly string restrictedUserPassword;

        protected BaseMySqlExecutionStrategy(
            string sysDbConnectionString,
            string restrictedUserId,
            string restrictedUserPassword)
        {
            if (string.IsNullOrWhiteSpace(sysDbConnectionString))
            {
                throw new ArgumentException("Invalid sys DB connection string!", nameof(sysDbConnectionString));
            }

            if (string.IsNullOrWhiteSpace(restrictedUserId))
            {
                throw new ArgumentException("Invalid restricted user ID!", nameof(restrictedUserId));
            }

            if (string.IsNullOrWhiteSpace(restrictedUserPassword))
            {
                throw new ArgumentException("Invalid restricted user password!", nameof(restrictedUserPassword));
            }

            this.sysDbConnectionString = sysDbConnectionString;
            this.restrictedUserId = restrictedUserId;
            this.restrictedUserPassword = restrictedUserPassword;
        }

        public override IDbConnection GetOpenConnection(string databaseName)
        {
            using (var connection = new MySqlConnection(this.sysDbConnectionString))
            {
                connection.Open();

                this.ExecuteNonQuery(connection, $"CREATE DATABASE `{databaseName}`;");

                this.ExecuteNonQuery(
                    connection,
                    $@"
CREATE USER IF NOT EXISTS '{this.restrictedUserId}'@'localhost';
SET PASSWORD FOR '{this.restrictedUserId}'@'localhost'='{this.restrictedUserPassword}'");

                this.ExecuteNonQuery(
                    connection,
                    $@"
GRANT ALL PRIVILEGES ON `{databaseName}`.* TO '{this.restrictedUserId}'@'localhost';
FLUSH PRIVILEGES;");
            }

            var createdDbConnectionString =
                $"Server=localhost;Database={databaseName};UID={this.restrictedUserId};Password={this.restrictedUserPassword};Pooling=False;";
            var createdDbConnection = new MySqlConnection(createdDbConnectionString);
            createdDbConnection.Open();

            return createdDbConnection;
        }

        public override void DropDatabase(string databaseName)
        {
            using (var connection = new MySqlConnection(this.sysDbConnectionString))
            {
                connection.Open();

                this.ExecuteNonQuery(connection, $"DROP DATABASE IF EXISTS `{databaseName}`;");
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
