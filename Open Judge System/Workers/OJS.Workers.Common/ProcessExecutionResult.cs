namespace OJS.Workers.Common
{
    using System;

    public class ProcessExecutionResult
    {
        public ProcessExecutionResult()
        {
            this.ReceivedOutput = string.Empty;
            this.ErrorOutput = string.Empty;
            this.ExitCode = 0;
            this.Type = ProcessExecutionResultType.Success;
            this.TimeWorked = new TimeSpan();
            this.MemoryUsed = 0;
        }

        public string ReceivedOutput { get; set; }

        public string ErrorOutput { get; set; }

        public int ExitCode { get; set; }

        public ProcessExecutionResultType Type { get; set; }

        public TimeSpan TimeWorked { get; set; }

        public long MemoryUsed { get; set; }

        public TimeSpan PrivilegedProcessorTime { get; set; }

        public TimeSpan UserProcessorTime { get; set; }

        public TimeSpan TotalProcessorTime
        {
            get
            {
                return this.PrivilegedProcessorTime + this.UserProcessorTime;
            }
        }
    }
}
