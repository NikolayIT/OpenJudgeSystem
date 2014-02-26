namespace OJS.Workers.Executors.JobObjects
{
    using System;
    using System.Runtime.InteropServices;

    public static class Win32
    {
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr windowHandler, out uint processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr obj);
    }
}
