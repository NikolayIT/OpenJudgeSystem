namespace OJS.Workers.Executors.Process
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public class SecurityAttributes
    {
        public int length = 12;

        public SafeLocalMemHandle lpSecurityDescriptor = new SafeLocalMemHandle(IntPtr.Zero, false);

        public bool bInheritHandle = false;
    }
}
