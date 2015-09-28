namespace OJS.Workers.Executors.Process
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// Specifies the window station, desktop, standard handles, and appearance of the main window for a process at creation time.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
    [StructLayout(LayoutKind.Sequential)]
    public class StartupInfo
    {
        public int SizeInBytes;

        public IntPtr Reserved = IntPtr.Zero;

        public IntPtr Desktop = IntPtr.Zero;

        public IntPtr Title = IntPtr.Zero;

        public int X = 0;

        public int Y = 0;

        public int XSize = 0;

        public int YSize = 0;

        public int XCountChars = 0;

        public int YCountChars = 0;

        public int FillAttribute = 0;

        public int Flags = 0;

        /// <summary>
        /// If dwFlags specifies STARTF_USESHOWWINDOW, this member can be any of the values that can be specified in the nCmdShow parameter for the ShowWindow function, except for SW_SHOWDEFAULT. Otherwise, this member is ignored.
        /// For GUI processes, the first time ShowWindow is called, its nCmdShow parameter is ignored wShowWindow specifies the default value. In subsequent calls to ShowWindow, the wShowWindow member is used if the nCmdShow parameter of ShowWindow is set to SW_SHOWDEFAULT.
        /// </summary>
        public short ShowWindow = 0;

        public short Reserved2 = 0;

        public IntPtr Reserved2Pointer = IntPtr.Zero;

        public SafeFileHandle StandardInputHandle = new SafeFileHandle(IntPtr.Zero, false);

        public SafeFileHandle StandardOutputHandle = new SafeFileHandle(IntPtr.Zero, false);

        public SafeFileHandle StandardErrorHandle = new SafeFileHandle(IntPtr.Zero, false);

        public StartupInfo()
        {
            this.SizeInBytes = Marshal.SizeOf(this);
        }

        public void Dispose()
        {
            // close the handles created for child process
            if (this.StandardInputHandle != null && !this.StandardInputHandle.IsInvalid)
            {
                this.StandardInputHandle.Close();
                this.StandardInputHandle = null;
            }

            if (this.StandardOutputHandle != null && !this.StandardOutputHandle.IsInvalid)
            {
                this.StandardOutputHandle.Close();
                this.StandardOutputHandle = null;
            }

            if (this.StandardErrorHandle != null && !this.StandardErrorHandle.IsInvalid)
            {
                this.StandardErrorHandle.Close();
                this.StandardErrorHandle = null;
            }
        }
    }
}
