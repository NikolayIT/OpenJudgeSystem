namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;

    using OJS.Common.Models;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;

    public class CSharpPerformanceProjectTestsExecutionStrategy : CSharpProjectTestsExecutionStrategy
    {
        public CSharpPerformanceProjectTestsExecutionStrategy(
            string nUnitConsoleRunnerPath,
            Func<CompilerType, string> getCompilerPathFunc)
            : base(nUnitConsoleRunnerPath, getCompilerPathFunc)
        {
            this.TestClassNames = new List<string>();
        }

        protected List<string> TestClassNames { get; }

        protected override ExecutionResult RunUnitTests(
            ExecutionContext executionContext,
            IExecutor executor,
            IChecker checker,
            ExecutionResult result,
            string compiledFile)
        {
            var testIndex = 0;
            foreach (var test in executionContext.Tests)
            {
                var arguments = new List<string> { $"--where \"class == {this.TestClassNames[testIndex]}\" \"{compiledFile}\"" };
                arguments.AddRange(AdditionalExecutionArguments.Split(' '));

                var processExecutionResult = executor.Execute(
                    this.NUnitConsoleRunnerPath,
                    string.Empty,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    arguments);

                var errorsByFiles = this.GetTestErrors(processExecutionResult.ReceivedOutput);

                var message = "Test Passed!";
                var testFile = this.TestNames[testIndex];
                if (errorsByFiles.ContainsKey(testFile))
                {
                    message = errorsByFiles[testFile];
                }

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, message);
                result.TestResults.Add(testResult);
                testIndex++;
            }

            return result;
        }

        protected override void ExtractTestNames(IEnumerable<TestContext> tests)
        {
            var trialTests = 1;
            var competeTests = 1;

            foreach (var test in tests)
            {
                var namespacePrefix = CSharpPreprocessorHelper.GetNamespaceName(test.Input);
                namespacePrefix = namespacePrefix == null ? string.Empty : namespacePrefix + ".";
                this.TestClassNames.Add($"{namespacePrefix}{CSharpPreprocessorHelper.GetClassName(test.Input)}");
                if (test.IsTrialTest)
                {
                    var testNumber = trialTests < 10 ? $"00{trialTests}" : $"0{trialTests}";
                    this.TestNames.Add($"{TrialTest}.{testNumber}");
                    trialTests++;
                }
                else
                {
                    var testNumber = competeTests < 10 ? $"00{competeTests}" : $"0{competeTests}";
                    this.TestNames.Add($"{CompeteTest}.{testNumber}");
                    competeTests++;
                }
            }
        }
    }
}
