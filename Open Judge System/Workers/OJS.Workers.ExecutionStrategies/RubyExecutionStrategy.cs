namespace OJS.Workers.ExecutionStrategies
{
    using System.IO;

    using OJS.Common.Extensions;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    public class RubyExecutionStrategy : ExecutionStrategy
    {
        protected const string SubmissionFileName = "_$Submission.rb";

        public RubyExecutionStrategy(string rubyPath)
        {
            this.RubyPath = rubyPath;
            this.WorkingDirectory = DirectoryHelpers.CreateTempDirectory();
        }

        ~RubyExecutionStrategy()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.WorkingDirectory, true);
        }

        public string RubyPath { get; set; }

        public string WorkingDirectory { get; set; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            result.IsCompiledSuccessfully = true;

            string submissionFullpath = $"{this.WorkingDirectory}\\{SubmissionFileName}";
            File.WriteAllText(submissionFullpath, executionContext.Code);

            var arguments = new[] { submissionFullpath };

            IExecutor executor = new RestrictedProcessExecutor();
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
                    arguments,
                    this.WorkingDirectory);

                var testResult = this.ExecuteAndCheckTest(
                    test,
                    processExecutionResult,
                    checker,
                    processExecutionResult.ReceivedOutput);

                result.TestResults.Add(testResult);
            }

            return result;
        }
    }
}
