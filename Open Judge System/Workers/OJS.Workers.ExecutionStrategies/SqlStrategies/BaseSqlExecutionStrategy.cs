namespace OJS.Workers.ExecutionStrategies.SqlStrategies
{
    using System;
    using System.Data;
    using System.Globalization;

    using MissingFeatures;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;

    public abstract class BaseSqlExecutionStrategy
    {
        protected static readonly Type DecimalType = typeof(decimal);
        protected static readonly Type DoubleType = typeof(double);
        protected static readonly Type FloatType = typeof(float);
        protected static readonly Type ByteArrayType = typeof(byte[]);
        protected static readonly Type DateTimeType = typeof(DateTime);
        protected static readonly Type TimeSpanType = typeof(TimeSpan);

        private const int DefaultTimeLimit = 60 * 1000;

        public virtual ExecutionResult Execute(
            ExecutionContext executionContext,
            Action<IDbConnection, TestContext, ExecutionResult> executionFlow)
        {
            var result = new ExecutionResult { IsCompiledSuccessfully = true };

            string databaseName = null;
            IDbConnection connection = null;
            try
            {
                executionContext.Tests.ForEach(test =>
                {
                    databaseName = this.GetDatabaseName();
                    connection = this.GetOpenConnection(databaseName);

                    executionFlow(connection, test, result);
                });
            }
            catch (Exception ex)
            {
                result.IsCompiledSuccessfully = false;
                result.CompilerComment = ex.Message;
            }
            finally
            {
                connection?.Dispose();
            }

            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                this.DropDatabase(databaseName);
            }

            return result;
        }

        protected abstract IDbConnection GetOpenConnection(string databaseName);

        protected abstract void DropDatabase(string databaseName);

        protected virtual string GetDatabaseName() => Guid.NewGuid().ToString();

        protected virtual string GetDataRecordFieldValue(IDataRecord dataRecord, int index)
        {
            string result;

            if (dataRecord.IsDBNull(index))
            {
                result = null;
            }
            else
            {
                var fieldType = dataRecord.GetFieldType(index);

                // Using CultureInfo.InvariantCulture to have consistent decimal separator.
                if (fieldType == DecimalType)
                {
                    result = dataRecord.GetDecimal(index).ToString(CultureInfo.InvariantCulture);
                }
                else if (fieldType == DoubleType)
                {
                    result = dataRecord.GetDouble(index).ToString(CultureInfo.InvariantCulture);
                }
                else if (fieldType == FloatType)
                {
                    result = dataRecord.GetFloat(index).ToString(CultureInfo.InvariantCulture);
                }
                else if (fieldType == ByteArrayType)
                {
                    var bytes = (byte[])dataRecord.GetValue(index);
                    result = bytes.ToHexString();
                }
                else
                {
                    result = dataRecord.GetValue(index).ToString();
                }
            }

            return result;
        }

        protected bool ExecuteNonQuery(IDbConnection connection, string commandText, int timeLimit = DefaultTimeLimit)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;

            return Code.ExecuteWithTimeLimit(
                TimeSpan.FromMilliseconds(timeLimit),
                () => command.ExecuteNonQuery());
        }

        protected SqlResult ExecuteReader(IDbConnection connection, string commandText, int timeLimit = DefaultTimeLimit)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;

            var sqlTestResult = new SqlResult();
            sqlTestResult.Completed = Code.ExecuteWithTimeLimit(
                TimeSpan.FromMilliseconds(timeLimit),
                () =>
                {
                    using (var reader = command.ExecuteReader())
                    {
                        do
                        {
                            while (reader.Read())
                            {
                                for (var i = 0; i < reader.FieldCount; i++)
                                {
                                    var fieldValue = this.GetDataRecordFieldValue(reader, i);

                                    sqlTestResult.Results.Add(fieldValue);
                                }
                            }
                        }
                        while (reader.NextResult());
                    }
                });

            return sqlTestResult;
        }

        protected void ProcessSqlResult(SqlResult sqlResult, ExecutionContext executionContext, TestContext test, ExecutionResult result)
        {
            if (sqlResult.Completed)
            {
                var joinedUserOutput = string.Join(Environment.NewLine, sqlResult.Results);

                var checker = Checker.CreateChecker(executionContext.CheckerAssemblyName, executionContext.CheckerTypeName, executionContext.CheckerParameter);
                var checkerResult = checker.Check(test.Input, joinedUserOutput, test.Output, test.IsTrialTest);

                result.TestResults.Add(new TestResult
                {
                    Id = test.Id,
                    ResultType = checkerResult.IsCorrect ? TestRunResultType.CorrectAnswer : TestRunResultType.WrongAnswer,
                    CheckerDetails = checkerResult.CheckerDetails
                });
            }
            else
            {
                result.TestResults.Add(new TestResult
                {
                    Id = test.Id,
                    TimeUsed = executionContext.TimeLimit,
                    ResultType = TestRunResultType.TimeLimit
                });
            }
        }
    }
}
