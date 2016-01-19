[assembly: log4net.Config.XmlConfigurator(Watch = true)]

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
    using OJS.Workers.Executors.Process;

    public class RestrictedProcessExecutor : IExecutor
    {
        private static ILog logger;

        public RestrictedProcessExecutor()
        {
            logger = LogManager.GetLogger(typeof(RestrictedProcessExecutor));
            //// logger.Info("Initialized.");
        }

        // TODO: double check and maybe change order of parameters
        public ProcessExecutionResult Execute(string fileName, string inputData, int timeLimit, int memoryLimit, IEnumerable<string> executionArguments = null)
        {
            var result = new ProcessExecutionResult { Type = ProcessExecutionResultType.Success };
            var workingDirectory = new FileInfo(fileName).DirectoryName;

            using (var restrictedProcess = new RestrictedProcess(fileName, workingDirectory, executionArguments, Math.Max(4096, (inputData.Length * 2) + 4)))
            {
                // Write to standard input using another thread
                restrictedProcess.StandardInput.WriteLineAsync(inputData).ContinueWith(
                    delegate
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        if (!restrictedProcess.IsDisposed)
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            restrictedProcess.StandardInput.FlushAsync().ContinueWith(
                                delegate
                                {
                                    restrictedProcess.StandardInput.Close();
                                });
                        }
                    });

                // Read standard output using another thread to prevent process locking (waiting us to empty the output buffer)
                var processOutputTask = restrictedProcess.StandardOutput.ReadToEndAsync().ContinueWith(
                    x =>
                    {
                        result.ReceivedOutput = x.Result;
                    });

                // Read standard error using another thread
                var errorOutputTask = restrictedProcess.StandardError.ReadToEndAsync().ContinueWith(
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
                            var peakWorkingSetSize = restrictedProcess.PeakWorkingSetSize;

                            result.MemoryUsed = Math.Max(result.MemoryUsed, peakWorkingSetSize);

                            if (memoryTaskCancellationToken.IsCancellationRequested)
                            {
                                return;
                            }

                            Thread.Sleep(TimeIntervalBetweenTwoMemoryConsumptionRequests);
                        }
                    },
                    memoryTaskCancellationToken.Token);

                // Start the process
                restrictedProcess.Start(timeLimit, memoryLimit);

                // Wait the process to complete. Kill it after (timeLimit * 1.5) milliseconds if not completed.
                // We are waiting the process for more than defined time and after this we compare the process time with the real time limit.
                var exited = restrictedProcess.WaitForExit((int)(timeLimit * 1.5));
                if (!exited)
                {
                    restrictedProcess.Kill();
                    restrictedProcess.WaitForExit(-1); // Wait indefinitely for the associated process to exit

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

                Debug.Assert(restrictedProcess.HasExited, "Restricted process didn't exit!");

                // Report exit code and total process working time
                result.ExitCode = restrictedProcess.ExitCode;
                result.TimeWorked = restrictedProcess.ExitTime - restrictedProcess.StartTime;
                result.PrivilegedProcessorTime = restrictedProcess.PrivilegedProcessorTime;
                result.UserProcessorTime = restrictedProcess.UserProcessorTime;
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
