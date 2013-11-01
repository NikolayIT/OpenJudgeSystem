namespace OJS.Workers.Executors.Process
{
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Threading;

    using Microsoft.Win32.SafeHandles;

    internal class ProcessWaitHandle : WaitHandle
    {
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal ProcessWaitHandle(SafeProcessHandle processHandle)
        {
            SafeWaitHandle waitHandle;
            bool succeeded = NativeMethods.DuplicateHandle(
                new HandleRef(this, NativeMethods.GetCurrentProcess()),
                processHandle,
                new HandleRef(this, NativeMethods.GetCurrentProcess()),
                out waitHandle,
                0,
                false,
                (int)DuplicateOptions.DUPLICATE_SAME_ACCESS);

            if (!succeeded)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            this.SafeWaitHandle = waitHandle;
        }
    }
}
