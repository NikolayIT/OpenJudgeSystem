namespace OJS.Workers.Common.ServiceInstaller
{
    using System;
    using System.Threading;

    using OJS.Workers.Common.ServiceInstaller.Models;

    public static class ServiceInstaller
    {
        private const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;

        public static bool ServiceIsInstalled(string serviceName)
        {
            var scm = OpenSCManager(ScmAccessRights.Connect);

            try
            {
                var service = SCNativeMethods.OpenService(scm, serviceName, ServiceAccessRights.QueryStatus);

                if (service == IntPtr.Zero)
                {
                    return false;
                }

                SCNativeMethods.CloseServiceHandle(service);
                return true;
            }
            finally
            {
                SCNativeMethods.CloseServiceHandle(scm);
            }
        }

        public static void InstallAndStart(string serviceName, string displayName, string fileName)
        {
            var scm = OpenSCManager(ScmAccessRights.AllAccess);

            try
            {
                var service = SCNativeMethods.OpenService(scm, serviceName, ServiceAccessRights.AllAccess);

                if (service == IntPtr.Zero)
                {
                    service = SCNativeMethods.CreateService(
                        scm,
                        serviceName,
                        displayName,
                        ServiceAccessRights.AllAccess,
                        SERVICE_WIN32_OWN_PROCESS,
                        ServiceBootFlag.AutoStart,
                        ServiceError.Normal,
                        fileName,
                        null,
                        IntPtr.Zero,
                        null,
                        null,
                        null);
                }

                if (service == IntPtr.Zero)
                {
                    throw new ApplicationException("Failed to install service.");
                }

                try
                {
                    StartService(service);
                }
                finally
                {
                    SCNativeMethods.CloseServiceHandle(service);
                }
            }
            finally
            {
                SCNativeMethods.CloseServiceHandle(scm);
            }
        }

        public static void StartService(string serviceName)
        {
            var scm = OpenSCManager(ScmAccessRights.Connect);

            try
            {
                var service = SCNativeMethods.OpenService(
                    scm,
                    serviceName,
                    ServiceAccessRights.QueryStatus | ServiceAccessRights.Start);

                if (service == IntPtr.Zero)
                {
                    throw new ApplicationException("Could not open service.");
                }

                try
                {
                    StartService(service);
                }
                finally
                {
                    SCNativeMethods.CloseServiceHandle(service);
                }
            }
            finally
            {
                SCNativeMethods.CloseServiceHandle(scm);
            }
        }

        public static void StopService(string serviceName)
        {
            var scm = OpenSCManager(ScmAccessRights.Connect);

            try
            {
                var service = SCNativeMethods.OpenService(
                    scm,
                    serviceName,
                    ServiceAccessRights.QueryStatus | ServiceAccessRights.Stop);

                if (service == IntPtr.Zero)
                {
                    throw new ApplicationException("Could not open service.");
                }

                try
                {
                    StopService(service);
                }
                finally
                {
                    SCNativeMethods.CloseServiceHandle(service);
                }
            }
            finally
            {
                SCNativeMethods.CloseServiceHandle(scm);
            }
        }

        public static ServiceState GetServiceStatus(string serviceName)
        {
            var scm = OpenSCManager(ScmAccessRights.Connect);

            try
            {
                var service = SCNativeMethods.OpenService(
                    scm,
                    serviceName,
                    ServiceAccessRights.QueryStatus);

                if (service == IntPtr.Zero)
                {
                    return ServiceState.NotFound;
                }

                try
                {
                    return GetServiceStatus(service);
                }
                finally
                {
                    SCNativeMethods.CloseServiceHandle(service);
                }
            }
            finally
            {
                SCNativeMethods.CloseServiceHandle(scm);
            }
        }

        private static void StartService(IntPtr service)
        {
            SCNativeMethods.StartService(service, 0, 0);
            var changedStatus = WaitForServiceStatus(service, ServiceState.StartPending, ServiceState.Running);
            if (!changedStatus)
            {
                throw new ApplicationException("Unable to start service");
            }
        }

        private static void StopService(IntPtr service)
        {
            var status = new ServiceStatus();
            SCNativeMethods.ControlService(service, ServiceControl.Stop, status);
            var changedStatus = WaitForServiceStatus(service, ServiceState.StopPending, ServiceState.Stopped);
            if (!changedStatus)
            {
                throw new ApplicationException("Unable to stop service");
            }
        }

        private static ServiceState GetServiceStatus(IntPtr service)
        {
            var status = new ServiceStatus();

            if (SCNativeMethods.QueryServiceStatus(service, status) == 0)
            {
                throw new ApplicationException("Failed to query service status.");
            }

            return status.dwCurrentState;
        }

        private static bool WaitForServiceStatus(IntPtr service, ServiceState waitStatus, ServiceState desiredStatus)
        {
            var status = new ServiceStatus();

            SCNativeMethods.QueryServiceStatus(service, status);
            if (status.dwCurrentState == desiredStatus)
            {
                return true;
            }

            var dwStartTickCount = Environment.TickCount;
            var dwOldCheckPoint = status.dwCheckPoint;

            while (status.dwCurrentState == waitStatus)
            {
                // Do not wait longer than the wait hint. A good interval is
                // one tenth the wait hint, but no less than 1 second and no
                // more than 10 seconds.
                var dwWaitTime = status.dwWaitHint / 10;

                if (dwWaitTime < 1000)
                {
                    dwWaitTime = 1000;
                }
                else if (dwWaitTime > 10000)
                {
                    dwWaitTime = 10000;
                }

                Thread.Sleep(dwWaitTime);

                // Check the status again.
                if (SCNativeMethods.QueryServiceStatus(service, status) == 0)
                {
                    break;
                }

                if (status.dwCheckPoint > dwOldCheckPoint)
                {
                    // The service is making progress.
                    dwStartTickCount = Environment.TickCount;
                    dwOldCheckPoint = status.dwCheckPoint;
                }
                else
                {
                    if (Environment.TickCount - dwStartTickCount > status.dwWaitHint)
                    {
                        // No progress made within the wait hint
                        break;
                    }
                }
            }

            return status.dwCurrentState == desiredStatus;
        }

        private static IntPtr OpenSCManager(ScmAccessRights rights)
        {
            var scm = SCNativeMethods.OpenSCManager(null, null, rights);
            if (scm == IntPtr.Zero)
            {
                throw new ApplicationException("Could not connect to service control manager.");
            }

            return scm;
        }
    }
}