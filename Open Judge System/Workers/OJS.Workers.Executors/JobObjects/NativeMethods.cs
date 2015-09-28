namespace OJS.Workers.Executors.JobObjects
{
    using System;
    using System.Runtime.InteropServices;

    public static class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateJobObject([In]ref SecurityAttributes jobAttributes, string name);

        [DllImport("kernel32.dll")]
        internal static extern bool SetInformationJobObject(
            IntPtr job,
            InfoClass infoType,
            IntPtr jobObjectInfo,
            uint jobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [DllImport("kernel32.dll")]
        internal static extern bool QueryInformationJobObject(
            IntPtr job,
            InfoClass jobObjectInformationClass,
            out IntPtr jobObjectInfo,
            uint jobObjectInfoLength,
            IntPtr returnLength);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr windowHandler, out uint processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr obj);
    }
}
