namespace OJS.Workers.ExecutionStrategies
{
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;

    public class CheckOnlyExecutionStrategy : ExecutionStrategy
    {
        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult
            {
                IsCompiledSuccessfully = true
            };

            var processExecutionResult = new ProcessExecutionResult
            {
                Type = ProcessExecutionResultType.Success,
                ReceivedOutput = executionContext.Code
            };

            var checker = Checker.CreateChecker(executionContext.CheckerAssemblyName, executionContext.CheckerTypeName, executionContext.CheckerParameter);

            foreach (var test in executionContext.Tests)
            {
                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, processExecutionResult.ReceivedOutput);
                result.TestResults.Add(testResult);
            }

            return result;
        }
    }
}
