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
            var attr = new SecurityAttributes();
            this.handle = CreateJobObject(ref attr, null);
        }

        public void SetExtendedLimitInformation(ExtendedLimitInformation extendedInfo)
        {
            var length = Marshal.SizeOf(typeof(ExtendedLimitInformation));
            var extendedInfoPointer = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(extendedInfo, extendedInfoPointer, false);
            if (!SetInformationJobObject(this.handle, InfoClass.ExtendedLimitInformation, extendedInfoPointer, (uint)length))
            {
                throw new Win32Exception();
            }
        }

        public void SetBasicUiRestrictions(BasicUiRestrictions uiRestrictions)
        {
            var length = Marshal.SizeOf(typeof(BasicUiRestrictions));
            var uiRestrictionsInfoPointer = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(uiRestrictions, uiRestrictionsInfoPointer, false);
            if (!SetInformationJobObject(this.handle, InfoClass.BasicUiRestrictions, uiRestrictionsInfoPointer, (uint)length))
            {
                throw new Win32Exception();
            }
        }

        public ExtendedLimitInformation GetExtendedLimitInformation()
        {
            var extendedLimitInformation = new ExtendedLimitInformation();
            var length = Marshal.SizeOf(typeof(ExtendedLimitInformation));
            var extendedLimitInformationInfoPointer = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(extendedLimitInformation, extendedLimitInformationInfoPointer, false);
            QueryInformationJobObject(this.handle, InfoClass.ExtendedLimitInformation, out extendedLimitInformationInfoPointer, (uint)length, IntPtr.Zero);
            return extendedLimitInformation;
        }

        //    //The peak memory used by any process ever associated with the job. 
        //IntPtr PeakProcessMemoryUsed
        //{
        //    get
        //    {
        //        ExtendedLimitInformation extendedLimitInformation =
        //            QueryJobInformation<JOBOBJECT_EXTENDED_LIMIT_INFORMATION, JobObjectExtendedLimitInformation>(_hJob);
        //        return System::IntPtr((void*)extendedLimitInformation.PeakProcessMemoryUsed);
        //    }
        //}
    
        ////The peak memory usage of all processes currently associated with the job.
        //System::IntPtr JobObject::PeakJobMemoryUsed::get()
        //{
        //    JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedLimitInformation = 
        //        QueryJobInformation<JOBOBJECT_EXTENDED_LIMIT_INFORMATION, JobObjectExtendedLimitInformation>(_hJob);
        //    return System::IntPtr((void *)extendedLimitInformation.PeakJobMemoryUsed);
        //}

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.Close();
            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            Win32.CloseHandle(this.handle);
            this.handle = IntPtr.Zero;
        }

        public bool AddProcess(IntPtr processHandle)
        {
            return AssignProcessToJobObject(this.handle, processHandle);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject([In]ref SecurityAttributes jobAttributes, string name);

        [DllImport("kernel32.dll")]
        private static extern bool SetInformationJobObject(
            IntPtr job,
            InfoClass infoType,
            IntPtr jobObjectInfo,
            uint jobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [DllImport("kernel32.dll")]
        private static extern bool QueryInformationJobObject(
            IntPtr hJob,
            InfoClass JobObjectInformationClass,
            out IntPtr lpJobObjectInfo,
            uint cbJobObjectInfoLength,
            IntPtr lpReturnLength);
    }
}