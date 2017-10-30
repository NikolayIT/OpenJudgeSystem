namespace OJS.Workers.ExecutionStrategies
{
    using System.Collections.Generic;
    using System.IO;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Workers.Checkers;
    using OJS.Workers.ExecutionStrategies.SqlStrategies.MySql;
    using OJS.Workers.Executors;

    public class PhpProjectWithDbExecutionStrategy : PhpProjectExecutionStrategy
    {
        protected const string DatabaseConfigurationFileName = "db.ini";
        protected const string TestRunnerClassName = "JudgeTestRunner";
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

        protected string ConnectionStringTemplate => @"dsn=""mysql:host=localhost;port=3306;dbname=##dbName##""
            user=""##username##""
            pass=""##password##""";

        protected string TestRunnerCodeTemplate => @"if(class_exists(""##testRunnerClassName##""))
    \##testRunnerClassName##::test();";

        protected BaseMySqlExecutionStrategy MySqlHelperStrategy { get; set; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();
            var databaseName = this.MySqlHelperStrategy.GetDatabaseName();

            // PHP code is not compiled
            result.IsCompiledSuccessfully = true;

            string submissionPath =
                $@"{this.WorkingDirectory}\\{ZippedSubmissionName}{GlobalConstants.ZipFileExtension}";
            File.WriteAllBytes(submissionPath, executionContext.FileContent);
            FileHelpers.UnzipFile(submissionPath, this.WorkingDirectory);
            File.Delete(submissionPath);

            this.ReplaceDatabaseConfigurationFile(databaseName);
            var applicationEntryPointPath = this.AddTestRunnerTemplateToApplicationEntryPoint();
            this.RequireSuperGlobalsTemplateInUserCode(applicationEntryPointPath);

            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result.TestResults = new List<TestResult>();

            var executor = new RestrictedProcessExecutor();
            foreach (var test in executionContext.Tests)
            {
                var dbConnection = this.MySqlHelperStrategy.GetOpenConnection(databaseName);
                dbConnection.Close();

                File.WriteAllText(this.SuperGlobalsTemplatePath, test.Input);

                var processExecutionResult = executor.Execute(
                    this.phpCliExecutablePath,
                    string.Empty,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    new[] { applicationEntryPointPath });

                var testResult = this.ExecuteAndCheckTest(
                    test,
                    processExecutionResult,
                    checker,
                    processExecutionResult.ReceivedOutput);

                result.TestResults.Add(testResult);
                this.MySqlHelperStrategy.DropDatabase(databaseName);
            }

            return result;
        }

        private string AddTestRunnerTemplateToApplicationEntryPoint()
        {
            var applicationEntryPointPath = FileHelpers.FindFileMatchingPattern(
                this.WorkingDirectory,
                ApplicationEntryPoint);
          
            var entryPointContent = File.ReadAllText(applicationEntryPointPath);

            var testRunnerCode = this.TestRunnerCodeTemplate.Replace("##testRunnerClassName##", TestRunnerClassName);
            entryPointContent += testRunnerCode;
            File.WriteAllText(applicationEntryPointPath, entryPointContent);

            return applicationEntryPointPath;
        }

        private void ReplaceDatabaseConfigurationFile(string databaseName)
        {
            var databaseConfiguration = this.ConnectionStringTemplate
                .Replace("##dbName##", databaseName)
                .Replace("##username##", this.restrictedUserId)
                .Replace("##password##", this.restrictedUserPassword)
                .Replace(" ", string.Empty);

            var databaseConfigurationFilePath = FileHelpers.FindFileMatchingPattern(
                this.WorkingDirectory,
                DatabaseConfigurationFileName);

            File.WriteAllText(databaseConfigurationFilePath, databaseConfiguration);
        }
    }
}
