namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using OJS.Common.Extensions;
    using OJS.Workers.Checkers;
    using OJS.Workers.Executors;

    public class PhpCgiExecuteAndCheckExecutionStrategy : ExecutionStrategy
    {
        private const string FileToExecuteOption = "--file";

        private readonly string phpCgiExecutablePath;

        public PhpCgiExecuteAndCheckExecutionStrategy(string phpCgiExecutablePath)
        {
            if (!File.Exists(phpCgiExecutablePath))
            {
                throw new ArgumentException(string.Format("PHP CGI not found in: {0}", phpCgiExecutablePath), "phpCgiExecutablePath");
            }

            this.phpCgiExecutablePath = phpCgiExecutablePath;
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            // PHP code is not compiled
            result.IsCompiledSuccessfully = true;

            var codeSavePath = FileHelpers.SaveStringToTempFile(executionContext.Code);

            // Process the submission and check each test
            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(executionContext.CheckerAssemblyName, executionContext.CheckerTypeName, executionContext.CheckerParameter);

            result.TestResults = new List<TestResult>();

            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(
                    this.phpCgiExecutablePath,
                    string.Empty, // Input data is passed as the last execution argument
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    new[] { FileToExecuteOption, codeSavePath, test.Input });

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, processExecutionResult.ReceivedOutput);
                result.TestResults.Add(testResult);
            }

            // Clean up
            File.Delete(codeSavePath);

            return result;
        }
    }
}
