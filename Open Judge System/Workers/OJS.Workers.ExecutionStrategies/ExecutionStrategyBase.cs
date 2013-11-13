namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;

    using OJS.Common.Models;
    using OJS.Workers.Common;

    public abstract class ExecutionStrategyBase : IExecutionStrategy
    {
        public abstract SubmissionsExecutorResult Execute(SubmissionsExecutorContext submission);

        protected TestResult ExecuteAndCheckTest(TestContext test, ProcessExecutionResult processExecutionResult, IChecker checker, string receivedOutput)
        {
            var testResult = new TestResult
            {
                Id = test.Id,
                TimeUsed = (int)processExecutionResult.TimeWorked.TotalMilliseconds,
                MemoryUsed = (int)processExecutionResult.MemoryUsed,
            };

            if (processExecutionResult.Type == ProcessExecutionResultType.RunTimeError)
            {
                testResult.ResultType = TestRunResultType.RunTimeError;
                testResult.ExecutionComment = processExecutionResult.ErrorOutput;
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.TimeLimit)
            {
                testResult.ResultType = TestRunResultType.TimeLimit;
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.MemoryLimit)
            {
                testResult.ResultType = TestRunResultType.MemoryLimit;
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.Success)
            {
                var checkerResult = checker.Check(test.Input, receivedOutput, test.Output);
                if (checkerResult.IsCorrect)
                {
                    testResult.ResultType = TestRunResultType.CorrectAnswer;
                }
                else
                {
                    testResult.ResultType = TestRunResultType.WrongAnswer;
                }

                // TODO: Do something with checkerResult.ResultType
                testResult.CheckerComment = checkerResult.CheckerDetails;
            }
            else
            {
                throw new ArgumentOutOfRangeException("processExecutionResult", "Invalid ProcessExecutionResultType value.");
            }

            return testResult;
        }

        protected string SaveStringToTempFile(string stringToWrite)
        {
            var code = stringToWrite;
            var tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, code);
            return tempFilePath;
        }
    }
}
