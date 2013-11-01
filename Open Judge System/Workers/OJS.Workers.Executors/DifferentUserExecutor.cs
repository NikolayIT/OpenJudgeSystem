namespace OJS.Workers.Executors
{
    using System;

    using OJS.Workers.Common;

    public class DifferentUserExecutor : IExecutor
    {
        // TODO: Move to app.config
        private const string UserName = @"testcode";
        private const string Password = @"testcode1234";

        public ProcessExecutionResult Execute(string fileName, string inputData, int timeLimit, int memoryLimit)
        {
            var process = new DifferentUserProcessExecutor(fileName, Environment.UserDomainName, UserName, Password);
            process.SetTextToWrite(inputData);

            var executorInfo = process.Start(timeLimit, memoryLimit);

            var result = new ProcessExecutionResult
                             {
                                 ReceivedOutput = executorInfo.StandardOutputContent,
                                 ErrorOutput = executorInfo.StandardErrorContent,
                                 ExitCode = process.Process.ExitCode,
                                 Type = ProcessExecutionResultType.Success,
                                 TimeWorked = process.Process.ExitTime - process.Process.StartTime,
                                 MemoryUsed = executorInfo.MaxMemoryUsed,
                                 PrivilegedProcessorTime = process.Process.PrivilegedProcessorTime,
                                 UserProcessorTime = process.Process.UserProcessorTime,
                             };

            if (executorInfo.ProcessKilledBecauseOfTimeLimit)
            {
                result.Type = ProcessExecutionResultType.TimeLimit;
            }

            if (!string.IsNullOrEmpty(executorInfo.StandardErrorContent))
            {
                result.Type = ProcessExecutionResultType.RunTimeError;
            }

            return result;
        }
    }
}
