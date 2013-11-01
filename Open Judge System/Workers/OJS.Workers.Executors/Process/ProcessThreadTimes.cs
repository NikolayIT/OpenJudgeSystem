namespace OJS.Workers.Executors.Process
{
    using System;

    internal struct ProcessThreadTimes
    {
        public long Create;
        public long Exit;
        public long Kernel;
        public long User;

        public DateTime StartTime
        {
            get
            {
                return DateTime.FromFileTime(this.Create);
            }
        }

        public DateTime ExitTime
        {
            get
            {
                return DateTime.FromFileTime(this.Exit);
            }
        }

        public TimeSpan PrivilegedProcessorTime
        {
            get
            {
                return new TimeSpan(this.Kernel);
            }
        }

        public TimeSpan UserProcessorTime
        {
            get
            {
                return new TimeSpan(this.User);
            }
        }

        public TimeSpan TotalProcessorTime
        {
            get
            {
                return new TimeSpan(this.User + this.Kernel);
            }
        }
    }
}