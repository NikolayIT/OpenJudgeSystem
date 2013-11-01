namespace OJS.Workers.Executors.Process
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Contains information about a newly created process and its primary thread.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessInformation
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }
}
