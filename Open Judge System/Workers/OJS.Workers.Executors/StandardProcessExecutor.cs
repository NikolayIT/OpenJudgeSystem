namespace OJS.Workers.Executors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using log4net;

    using OJS.Workers.Common;

    // TODO: Implement memory constraints
    public class StandardProcessExecutor : IExecutor
    {
        private static ILog logger;

        public StandardProcessExecutor()
        {
            logger = LogManager.GetLogger(typeof(StandardProcessExecutor));
            //// logger.Info("Initialized.");
        }

        public ProcessExecutionResult Execute(string fileName, string inputData, int timeLimit, int memoryLimit, IEnumerable<string> executionArguments = null)
        {
            var result = new ProcessExecutionResult { Type = ProcessExecutionResultType.Success };
            var workingDirectory = new FileInfo(fileName).DirectoryName;

            var processStartInfo = new ProcessStartInfo(fileName)
            {
                Arguments = executionArguments == null ? string.Empty : string.Join(" ", executionArguments),
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                ErrorDialog = false,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                WorkingDirectory = workingDirectory
            };

            using (var process = System.Diagnostics.Process.Start(processStartInfo))
            {
                if (process == null)
                {
                    var errorMessage = string.Format("Could not start process: {0}!", fileName);
                    throw new Exception(errorMessage);
                }

                // Write to standard input using another thread
                process.StandardInput.WriteLineAsync(inputData).ContinueWith(
                    delegate
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        process.StandardInput.FlushAsync().ContinueWith(
                            delegate
                            {
                                process.StandardInput.Close();
                            });
                    });

                // Read standard output using another thread to prevent process locking (waiting us to empty the output buffer)
                var processOutputTask = process.StandardOutput.ReadToEndAsync().ContinueWith(
                    x =>
                    {
                        result.ReceivedOutput = x.Result;
                    });

                // Read standard error using another thread
                var errorOutputTask = process.StandardError.ReadToEndAsync().ContinueWith(
                    x =>
                    {
                        result.ErrorOutput = x.Result;
                    });

                // Read memory consumption every few milliseconds to determine the peak memory usage of the process
                const int TimeIntervalBetweenTwoMemoryConsumptionRequests = 45;
                var memoryTaskCancellationToken = new CancellationTokenSource();
                var memoryTask = Task.Run(
                    () =>
                    {
                        while (true)
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            var peakWorkingSetSize = process.PeakWorkingSet64;

                            result.MemoryUsed = Math.Max(result.MemoryUsed, peakWorkingSetSize);

                            if (memoryTaskCancellationToken.IsCancellationRequested)
                            {
                                return;
                            }

                            Thread.Sleep(TimeIntervalBetweenTwoMemoryConsumptionRequests);
                        }
                    },
                    memoryTaskCancellationToken.Token);

                // Wait the process to complete. Kill it after (timeLimit * 1.5) milliseconds if not completed.
                // We are waiting the process for more than defined time and after this we compare the process time with the real time limit.
                var exited = process.WaitForExit((int)(timeLimit * 1.5));
                if (!exited)
                {
                    process.Kill();
                    result.Type = ProcessExecutionResultType.TimeLimit;
                }

                // Close the memory consumption check thread
                memoryTaskCancellationToken.Cancel();
                try
                {
                    // To be sure that memory consumption will be evaluated correctly
                    memoryTask.Wait(TimeIntervalBetweenTwoMemoryConsumptionRequests);
                }
                catch (AggregateException ex)
                {
                    logger.Warn("AggregateException caught.", ex.InnerException);
                }

                // Close the task that gets the process error output
                try
                {
                    errorOutputTask.Wait(100);
                }
                catch (AggregateException ex)
                {
                    logger.Warn("AggregateException caught.", ex.InnerException);
                }

                // Close the task that gets the process output
                try
                {
                    processOutputTask.Wait(100);
                }
                catch (AggregateException ex)
                {
                    logger.Warn("AggregateException caught.", ex.InnerException);
                }

                Debug.Assert(process.HasExited, "Standard process didn't exit!");

                // Report exit code and total process working time
                result.ExitCode = process.ExitCode;
                result.TimeWorked = process.ExitTime - process.StartTime;
                result.PrivilegedProcessorTime = process.PrivilegedProcessorTime;
                result.UserProcessorTime = process.UserProcessorTime;
            }

            if (result.TotalProcessorTime.TotalMilliseconds > timeLimit)
            {
                result.Type = ProcessExecutionResultType.TimeLimit;
            }

            if (!string.IsNullOrEmpty(result.ErrorOutput))
            {
                result.Type = ProcessExecutionResultType.RunTimeError;
            }

            if (result.MemoryUsed > memoryLimit)
            {
                result.Type = ProcessExecutionResultType.MemoryLimit;
            }

            return result;
        }
    }
}
