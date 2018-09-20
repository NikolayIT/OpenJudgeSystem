namespace OJS.Workers.ExecutionStrategies
{
    using System.IO;

    using OJS.Workers.Checkers;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Executors;

    public class RubyExecutionStrategy : ExecutionStrategy
    {
        public RubyExecutionStrategy(string rubyPath, int baseTimeUsed, int baseMemoryUsed)
            : base(baseTimeUsed, baseMemoryUsed) =>
                this.RubyPath = rubyPath;

        public string RubyPath { get; set; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            result.IsCompiledSuccessfully = true;

            var submissionFilePath = FileHelpers.SaveStringToTempFile(this.WorkingDirectory, executionContext.Code);

            var arguments = new[] { submissionFilePath };

            var executor = new RestrictedProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(
                    this.RubyPath,
                    test.Input,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    arguments);

                var testResult = this.ExecuteAndCheckTest(
                    test,
                    processExecutionResult,
                    checker,
                    processExecutionResult.ReceivedOutput);

                result.TestResults.Add(testResult);
            }

            if (File.Exists(submissionFilePath))
            {
                File.Delete(submissionFilePath);
            }

            return result;
        }
    }
}
