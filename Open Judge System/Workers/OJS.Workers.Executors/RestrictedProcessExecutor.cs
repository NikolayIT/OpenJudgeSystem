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

    using OJS.Common;
    using OJS.Workers.Common;
    using OJS.Workers.Executors.Process;

    public class RestrictedProcessExecutor : IExecutor
    {
        private const int TimeIntervalBetweenTwoMemoryConsumptionRequests = 45;
        private const int TimeBeforeClosingOutputStreams = 300;

        private static ILog logger;

        private readonly int baseTimeUsed;
        private readonly int baseMemoryUsed;

        public RestrictedProcessExecutor() =>
            logger = LogManager.GetLogger(typeof(RestrictedProcessExecutor));

        /// <summary>
        /// Initializes a new instance of the RestrictedProcessExecutor class with base time and memory used
        /// </summary>
        /// <param name="baseTimeUsed">The base time in milliseconds added to the time limit when executing</param>
        /// <param name="baseMemoryUsed">The base memory in bytes added to the memory limit when executing</param>
        public RestrictedProcessExecutor(int baseTimeUsed, int baseMemoryUsed)
            : this()
        {
            this.baseTimeUsed = baseTimeUsed;
            this.baseMemoryUsed = baseMemoryUsed;
        }

        public ProcessExecutionResult Execute(
            string fileName,
            string inputData,
            int timeLimit,
            int memoryLimit,
            IEnumerable<string> executionArguments = null,
            string workingDirectory = null,
            bool useProcessTime = false,
            bool useSystemEncoding = false,
            double timeoutMultiplier = 1.5)
        {
            timeLimit = timeLimit + this.baseTimeUsed;
            memoryLimit = memoryLimit + this.baseMemoryUsed;

            var result = new ProcessExecutionResult { Type = ProcessExecutionResultType.Success };
            if (workingDirectory == null)
            {
                workingDirectory = new FileInfo(fileName).DirectoryName;
            }

            using (var restrictedProcess = new RestrictedProcess(fileName, workingDirectory, executionArguments, Math.Max(4096, (inputData.Length * 2) + 4), useSystemEncoding))
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
                var exited = restrictedProcess.WaitForExit((int)(timeLimit * timeoutMultiplier));
                if (!exited)
                {
                    restrictedProcess.Kill();

                    // Wait for the associated process to exit before continuing
                    restrictedProcess.WaitForExit(GlobalConstants.DefaultProcessExitTimeOutMilliseconds);

                    result.ProcessWasKilled = true;
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
                    logger.Warn($"AggregateException caught in Memory Sampling Thread. Inner Exception: {ex.InnerException}");
                }

                // Close the task that gets the process error output
                try
                {
                    errorOutputTask.Wait(TimeBeforeClosingOutputStreams);
                }
                catch (AggregateException ex)
                {
                    logger.Warn($"AggregateException caught in Error Output Thread. Inner Exception: {ex.InnerException}");
                }

                // Close the task that gets the process output
                try
                {
                    processOutputTask.Wait(TimeBeforeClosingOutputStreams);
                }
                catch (AggregateException ex)
                {
                    logger.Warn($"AggregateException caught in Standard Output Thread. Inner Exception: {ex.InnerException}");
                }

                Debug.Assert(restrictedProcess.HasExited, "Restricted process didn't exit!");

                // Report exit code and total process working time
                result.ExitCode = restrictedProcess.ExitCode;
                result.TimeWorked = restrictedProcess.ExitTime - restrictedProcess.StartTime;
                result.PrivilegedProcessorTime = restrictedProcess.PrivilegedProcessorTime;
                result.UserProcessorTime = restrictedProcess.UserProcessorTime;
            }

            if (useProcessTime)
            {
                if (result.TimeWorked.TotalMilliseconds > timeLimit)
                {
                    result.Type = ProcessExecutionResultType.TimeLimit;
                }
            }
            else
            {
                if (result.TotalProcessorTime.TotalMilliseconds > timeLimit)
                {
                    result.Type = ProcessExecutionResultType.TimeLimit;
                }
            }

            if (!string.IsNullOrEmpty(result.ErrorOutput))
            {
                result.Type = ProcessExecutionResultType.RunTimeError;
            }
            else if (result.ExitCode != 0 && result.ExitCode != -1)
            {
                result.Type = ProcessExecutionResultType.RunTimeError;
            }

            if (result.MemoryUsed > memoryLimit)
            {
                result.Type = ProcessExecutionResultType.MemoryLimit;
            }

            result.ApplyTimeAndMemoryOffset(this.baseTimeUsed, this.baseMemoryUsed);

            return result;
        }
    }
}