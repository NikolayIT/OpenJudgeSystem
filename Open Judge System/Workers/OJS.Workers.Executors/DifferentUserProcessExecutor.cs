namespace OJS.Workers.Executors
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using OJS.Common.Extensions;
    using OJS.Workers.Executors.JobObjects;
    using OJS.Workers.Executors.Process;

    /// <summary>
    /// Wrapper over existing System.Diagnostics.Process class.
    /// Attaches the process to restricted job object.
    /// </summary>
    public class DifferentUserProcessExecutor
    {
        private readonly System.Diagnostics.Process process;

        private char[] charsToWrite;

        public DifferentUserProcessExecutor(string fileName, string domain, string userName, string password)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                WorkingDirectory =
                    new FileInfo(fileName).DirectoryName ?? string.Empty,
                Domain = domain,
                UserName = userName,
                Password = password.ToSecureString(),
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
            };
            this.process = new System.Diagnostics.Process { StartInfo = startInfo };
            this.process.Exited += this.ProcessOnExited;
        }

        public System.Diagnostics.Process Process
        {
            get
            {
                return this.process;
            }
        }

        public char[] CharsToWrite
        {
            get
            {
                return this.charsToWrite;
            }
        }

        public long PeakWorkingSetSize
        {
            get
            {
                var counters = new ProcessMemoryCounters();
                NativeMethods.GetProcessMemoryInfo(this.process.Handle, out counters, (uint)Marshal.SizeOf(counters));
                return (int)counters.PeakWorkingSetSize;
            }
        }

        public long PeakPagefileUsage
        {
            get
            {
                var counters = new ProcessMemoryCounters();
                NativeMethods.GetProcessMemoryInfo(this.process.Handle, out counters, (uint)Marshal.SizeOf(counters));
                return (int)counters.PeakPagefileUsage;
            }
        }

        public void SetTextToWrite(string textToWrite)
        {
            this.charsToWrite = Encoding.ASCII.GetChars(Encoding.ASCII.GetBytes(textToWrite));
            //// TODO: Use utf8?
        }

        public ProcessExecutionInfo Start(int timeLimit, int memoryLimit)
        {
            var executionInfo = new ProcessExecutionInfo();

            using (var job = new JobObject())
            {
                // Prepare job
                job.SetExtendedLimitInformation(PrepareJobObject.GetExtendedLimitInformation(timeLimit, memoryLimit));
                job.SetBasicUiRestrictions(PrepareJobObject.GetUiRestrictions());

                // Start process and assign it to job object
                this.process.Start();

                var memoryTaskCancellationToken = new CancellationTokenSource();
                var memoryTask = Task.Run(
                    () =>
                    {
                        while (true)
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            var peakWorkingSetSize = this.PeakWorkingSetSize;

                            executionInfo.MaxMemoryUsed = Math.Max(executionInfo.MaxMemoryUsed, peakWorkingSetSize);

                            if (memoryTaskCancellationToken.IsCancellationRequested)
                            {
                                return;
                            }

                            Thread.Sleep(30);
                        }
                    },
                    memoryTaskCancellationToken.Token);

                job.AddProcess(this.process.Handle);
                this.process.PriorityClass = ProcessPriorityClass.RealTime;

                // Process input
                if (this.charsToWrite != null)
                {
                    try
                    {
                        this.process.StandardInput.WriteAsync(this.charsToWrite, 0, this.charsToWrite.Length)
                            .ContinueWith(
                                delegate
                                    {
                                        // this.process.StandardInput.AutoFlush = false;
                                        this.process.StandardInput.FlushAsync();
                                    });
                    }
                    catch (IOException)
                    {
                        // The pipe has been ended exception (when process is stopped before the write is done)
                    }
                }

                var exited = this.process.WaitForExit(timeLimit);
                if (!exited)
                {
                    // TODO: Fix: Console.WriteLine(job.GetExtendedLimitInformation().BasicLimitInformation.ActiveProcessLimit);
                    job.Close();
                    executionInfo.ProcessKilledBecauseOfTimeLimit = true; // Time limit
                }

                // Process output
                var output = this.process.StandardOutput.ReadToEnd();

                // Process error
                var errorOutput = this.process.StandardError.ReadToEnd();

                memoryTaskCancellationToken.Cancel();
                memoryTask.Wait(30); // To be sure that memory consumption will be evaluated correctly
                
                // Prepare execution info
                executionInfo.StandardOutputContent = output;
                executionInfo.StandardErrorContent = errorOutput;
            }

            return executionInfo;
        }

        private void ProcessOnExited(object sender, EventArgs eventArgs)
        {
        }

        public class ProcessExecutionInfo
        {
            public string StandardOutputContent { get; set; }

            public string StandardErrorContent { get; set; }

            public bool ProcessKilledBecauseOfTimeLimit { get; set; }

            public long MaxMemoryUsed { get; set; }
        }
    }
}
