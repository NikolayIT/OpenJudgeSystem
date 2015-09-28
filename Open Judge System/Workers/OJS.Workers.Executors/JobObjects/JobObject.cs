namespace OJS.Workers.Executors.JobObjects
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    public class JobObject : IDisposable
    {
        private IntPtr handle;
        private bool disposed;

        public JobObject()
        {
            var attr = default(SecurityAttributes);
            this.handle = NativeMethods.CreateJobObject(ref attr, null);
        }

        ~JobObject()
        {
            this.Dispose(false);
        }

        public void SetExtendedLimitInformation(ExtendedLimitInformation extendedInfo)
        {
            var length = Marshal.SizeOf(typeof(ExtendedLimitInformation));
            var extendedInfoPointer = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(extendedInfo, extendedInfoPointer, false);
            if (!NativeMethods.SetInformationJobObject(this.handle, InfoClass.ExtendedLimitInformation, extendedInfoPointer, (uint)length))
            {
                throw new Win32Exception();
            }
        }

        public void SetBasicUiRestrictions(BasicUiRestrictions uiRestrictions)
        {
            var length = Marshal.SizeOf(typeof(BasicUiRestrictions));
            var uiRestrictionsInfoPointer = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(uiRestrictions, uiRestrictionsInfoPointer, false);
            if (!NativeMethods.SetInformationJobObject(this.handle, InfoClass.BasicUiRestrictions, uiRestrictionsInfoPointer, (uint)length))
            {
                throw new Win32Exception();
            }
        }

        public ExtendedLimitInformation GetExtendedLimitInformation()
        {
            var extendedLimitInformation = default(ExtendedLimitInformation);
            var length = Marshal.SizeOf(typeof(ExtendedLimitInformation));
            var extendedLimitInformationInfoPointer = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(extendedLimitInformation, extendedLimitInformationInfoPointer, false);
            NativeMethods.QueryInformationJobObject(this.handle, InfoClass.ExtendedLimitInformation, out extendedLimitInformationInfoPointer, (uint)length, IntPtr.Zero);
            return extendedLimitInformation;
        }

        //// // The peak memory used by any process ever associated with the job.
        //// IntPtr PeakProcessMemoryUsed
        //// {
        ////     get
        ////     {
        ////         ExtendedLimitInformation extendedLimitInformation =
        ////             QueryJobInformation<JOBOBJECT_EXTENDED_LIMIT_INFORMATION, JobObjectExtendedLimitInformation>(_hJob);
        ////         return System::IntPtr((void*)extendedLimitInformation.PeakProcessMemoryUsed);
        ////     }
        //// }

        //// // The peak memory usage of all processes currently associated with the job.
        //// System::IntPtr JobObject::PeakJobMemoryUsed::get()
        //// {
        ////     JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedLimitInformation =
        ////         QueryJobInformation<JOBOBJECT_EXTENDED_LIMIT_INFORMATION, JobObjectExtendedLimitInformation>(_hJob);
        ////     return System::IntPtr((void *)extendedLimitInformation.PeakJobMemoryUsed);
        //// }

        public void Close()
        {
            NativeMethods.CloseHandle(this.handle);
            this.handle = IntPtr.Zero;
        }

        public bool AddProcess(IntPtr processHandle)
        {
            return NativeMethods.AssignProcessToJobObject(this.handle, processHandle);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.disposed)
                {
                    return;
                }

                this.Close();
                this.disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}