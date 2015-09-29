namespace OJS.Workers.Executors.Process
{
    using System;

    internal struct ProcessThreadTimes
    {
        public long Create;
        public long Exit;
        public long Kernel;
        public long User;

        public DateTime StartTime => DateTime.FromFileTime(this.Create);

        public DateTime ExitTime => DateTime.FromFileTime(this.Exit);

        public TimeSpan PrivilegedProcessorTime => new TimeSpan(this.Kernel);

        public TimeSpan UserProcessorTime => new TimeSpan(this.User);

        public TimeSpan TotalProcessorTime => new TimeSpan(this.User + this.Kernel);
    }
}
