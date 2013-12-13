namespace OJS.Workers.Executors.Process
{
    using System;
    using System.Runtime.InteropServices;

    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// Specifies the window station, desktop, standard handles, and appearance of the main window for a process at creation time. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class StartupInfo
    {
        public int cb;

        public IntPtr lpReserved = IntPtr.Zero;

        public IntPtr lpDesktop = IntPtr.Zero;

        public IntPtr lpTitle = IntPtr.Zero;

        public int dwX = 0;

        public int dwY = 0;

        public int dwXSize = 0;

        public int dwYSize = 0;

        public int dwXCountChars = 0;

        public int dwYCountChars = 0;

        public int dwFillAttribute = 0;

        public int dwFlags = 0;

        /// <summary>
        /// If dwFlags specifies STARTF_USESHOWWINDOW, this member can be any of the values that can be specified in the nCmdShow parameter for the ShowWindow function, except for SW_SHOWDEFAULT. Otherwise, this member is ignored.
        /// For GUI processes, the first time ShowWindow is called, its nCmdShow parameter is ignored wShowWindow specifies the default value. In subsequent calls to ShowWindow, the wShowWindow member is used if the nCmdShow parameter of ShowWindow is set to SW_SHOWDEFAULT.
        /// </summary>
        public short wShowWindow = 0;

        public short cbReserved2 = 0;

        public IntPtr lpReserved2 = IntPtr.Zero;

        public SafeFileHandle standardInputHandle = new SafeFileHandle(IntPtr.Zero, false);

        public SafeFileHandle standardOutputHandle = new SafeFileHandle(IntPtr.Zero, false);

        public SafeFileHandle standardErrorHandle = new SafeFileHandle(IntPtr.Zero, false);

        public StartupInfo()
        {
            this.cb = Marshal.SizeOf(this);
        }

        public void Dispose()
        {
            // close the handles created for child process
            if (this.standardInputHandle != null && !this.standardInputHandle.IsInvalid)
            {
                this.standardInputHandle.Close();
                this.standardInputHandle = null;
            }

            if (this.standardOutputHandle != null && !this.standardOutputHandle.IsInvalid)
            {
                this.standardOutputHandle.Close();
                this.standardOutputHandle = null;
            }

            if (this.standardErrorHandle != null && !this.standardErrorHandle.IsInvalid)
            {
                this.standardErrorHandle.Close();
                this.standardErrorHandle = null;
            }
        }
    }
}
