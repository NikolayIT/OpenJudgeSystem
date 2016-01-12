namespace OJS.Workers.Executors.Process
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
    [StructLayout(LayoutKind.Sequential)]
    public class SecurityAttributes
    {
        public int Length = 12;

        public SafeLocalMemHandle SecurityDescriptor = new SafeLocalMemHandle(IntPtr.Zero, false);

        public bool InheritHandle = false;
    }
}
