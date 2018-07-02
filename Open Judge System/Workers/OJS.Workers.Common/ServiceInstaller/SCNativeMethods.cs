namespace OJS.Workers.Common.ServiceInstaller
{
    using System;
    using System.Runtime.InteropServices;

    using OJS.Workers.Common.ServiceInstaller.Models;

    internal static class SCNativeMethods
    {
        [DllImport(
            "advapi32.dll",
            EntryPoint = "OpenSCManagerW",
            ExactSpelling = true,
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        internal static extern IntPtr OpenSCManager(
            string machineName,
            string databaseName,
            ScmAccessRights dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr OpenService(
            IntPtr hSCManager,
            string lpServiceName,
            ServiceAccessRights dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateService(
            IntPtr hSCManager,
            string lpServiceName,
            string lpDisplayName,
            ServiceAccessRights dwDesiredAccess,
            int dwServiceType,
            ServiceBootFlag dwStartType,
            ServiceError dwErrorControl,
            string lpBinaryPathName,
            string lpLoadOrderGroup,
            IntPtr lpdwTagId,
            string lpDependencies,
            string lp,
            string lpPassword);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseServiceHandle(IntPtr hSCObject);

        [DllImport("advapi32.dll")]
        internal static extern int QueryServiceStatus(
            IntPtr hService,
            ServiceStatus lpServiceStatus);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteService(IntPtr hService);

        [DllImport("advapi32.dll")]
        internal static extern int ControlService(
            IntPtr hService,
            ServiceControl dwControl,
            ServiceStatus lpServiceStatus);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern int StartService(
            IntPtr hService,
            int dwNumServiceArgs,
            int lpServiceArgVectors);
    }
}
