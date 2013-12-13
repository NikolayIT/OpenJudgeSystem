namespace OJS.Workers.Executors.Process
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public class SecurityAttributes
    {
        public const int Length = 12;

        public readonly SafeLocalMemHandle SecurityDescriptor = new SafeLocalMemHandle(IntPtr.Zero, false);

        public bool InheritHandle = false;
    }
}
