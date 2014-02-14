using System;
using System.Runtime.InteropServices;

using Asprosys.Win32;

namespace Asprosys.Win32
{

    public delegate bool ReleaseHandleCallback(IntPtr handle);

    public class GenericSafeHandle : SafeHandle
    {
        private ReleaseHandleCallback m_ReleaseCallback;

        #region Constructors

        public GenericSafeHandle(IntPtr handle, ReleaseHandleCallback releaseCallback)
            : base(IntPtr.Zero, true)
        {
            base.handle = handle;
            m_ReleaseCallback = releaseCallback;
        }

        public GenericSafeHandle(IntPtr handle)
            : this(handle, NativeMethods.CloseHandle)
        {
        }

        public GenericSafeHandle(IntPtr newHandle, GenericSafeHandle oldHandle)
            : this(newHandle, oldHandle.m_ReleaseCallback)
        {
            oldHandle.Close();
        }

        #endregion

        public override bool IsInvalid
        {
            get { return (handle == IntPtr.Zero); }
        }

        protected override bool ReleaseHandle()
        {
            return m_ReleaseCallback(handle);
        }
    }
}
