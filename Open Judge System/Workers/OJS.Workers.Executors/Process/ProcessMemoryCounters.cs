namespace OJS.Workers.Executors.Process
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessMemoryCounters
    {
        public int SizeInBytes;
        public int PageFaultCount;
        public IntPtr PeakWorkingSetSize;
        public IntPtr WorkingSetSize;
        public IntPtr QuotaPeakPagedPoolUsage;
        public IntPtr QuotaPagedPoolUsage;
        public IntPtr QuotaPeakNonPagedPoolUsage;
        public IntPtr QuotaNonPagedPoolUsage;
        public IntPtr PagefileUsage;
        public IntPtr PeakPagefileUsage;
    }
}