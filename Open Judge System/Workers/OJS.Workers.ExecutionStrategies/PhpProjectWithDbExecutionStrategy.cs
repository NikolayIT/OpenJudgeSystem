namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;
    using System.Linq;
    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Workers.ExecutionStrategies.SqlStrategies.MySql;

    public class PhpProjectWithDbExecutionStrategy : PhpProjectExecutionStrategy
    {
        protected const string DatabaseConfigurationFileName = "db.ini";
        private readonly string restrictedUserId;
        private readonly string restrictedUserPassword;

        public PhpProjectWithDbExecutionStrategy(
            string phpCliExecutablePath,
            string sysDbConnectionString,
            string restrictedUserId,
            string restrictedUserPassword)
            : base(phpCliExecutablePath)
        {
            this.MySqlHelperStrategy = new MySqlPrepareDatabaseAndRunQueriesExecutionStrategy(
                sysDbConnectionString,
                restrictedUserId,
                restrictedUserPassword);

            this.restrictedUserId = restrictedUserId;
            this.restrictedUserPassword = restrictedUserPassword;
        }

        protected string ConnectionStringTemplate => @"
            dsn=""mysql:host=localhost;port=3306;dbname=##dbName##""
            user=""##username##""
            pass=""##password##""";

        protected BaseMySqlExecutionStrategy MySqlHelperStrategy { get; set; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();
            var databaseName = this.MySqlHelperStrategy.GetDatabaseName();
            var connectionString = string.Empty;

            var databaseConfiguration = this.ConnectionStringTemplate
                .Replace("##dbname##", databaseName)
                .Replace("##username##", this.restrictedUserId)
                .Replace("##password##", this.restrictedUserPassword);

            string submissionPath =
                $@"{this.WorkingDirectory}\\{ZippedSubmissionName}{GlobalConstants.ZipFileExtension}";
            File.WriteAllBytes(submissionPath, executionContext.FileContent);

            var dbIniZipPath = FileHelpers.GetFilePathsFromZip(submissionPath)
                .FirstOrDefault(f => f.EndsWith(DatabaseConfigurationFileName));

            if (string.IsNullOrEmpty(dbIniZipPath))
            {
                throw new ArgumentException($"{DatabaseConfigurationFileName} not found in the submission");
            }

            var databaseConfigurationPath = $"{this.WorkingDirectory}\\{DatabaseConfigurationFileName}";
            File.WriteAllText(databaseConfigurationPath, databaseConfiguration);
            FileHelpers.AddFilesToZipArchive(submissionPath, dbIniZipPath, databaseConfigurationPath);

            using (var connection = this.MySqlHelperStrategy.GetOpenConnection(databaseName))
            {
                connectionString = connection.ConnectionString;
            }

            result.IsCompiledSuccessfully = true;
            return null;

        }
    }

}
