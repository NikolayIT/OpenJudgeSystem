namespace OJS.Workers.Executors.Process
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    using Microsoft.Win32.SafeHandles;

    [SuppressUnmanagedCodeSecurity]
    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public sealed class SafeLocalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeLocalMemHandle()
            : base(true)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeLocalMemHandle(IntPtr existingHandle, bool ownsHandle)
            : base(ownsHandle)
        {
            this.SetHandle(existingHandle);
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.LocalFree(this.handle) == IntPtr.Zero;
        }
    }
}
