namespace OJS.Common.Helpers
{
    using System;
    using System.Configuration.Install;
    using System.Linq;
    using System.ServiceProcess;

    using OJS.Common.Models;

    public static class ServicesHelper
    {
        private const int MaxWaitTimeInMilliseconds = 10000;
        private static readonly TimeSpan WaitTimeOut =
            TimeSpan.FromMilliseconds(MaxWaitTimeInMilliseconds);

        public static void InstallService(string serviceName, string exeFilePath)
        {
            var installer = new AssemblyInstaller(exeFilePath, null)
            {
                UseNewContext = true
            };

            installer.Install(null);
            installer.Commit(null);

            if (!ServiceIsInstalled(serviceName))
            {
                throw new ArgumentException($"Unable to install \"{serviceName}\" service.");
            }
        }

        public static void StartService(string serviceName)
        {
            using (var serviceController = new ServiceController(serviceName))
            {
                switch (serviceController.Status)
                {
                    case ServiceControllerStatus.Running:
                        return;
                    case ServiceControllerStatus.StartPending:
                    case ServiceControllerStatus.ContinuePending:
                        break;
                    case ServiceControllerStatus.Paused:
                        serviceController.Continue();
                        break;
                    case ServiceControllerStatus.PausePending:
                        serviceController.WaitForStatus(ServiceControllerStatus.Paused, WaitTimeOut);
                        goto case ServiceControllerStatus.Paused;
                    case ServiceControllerStatus.Stopped:
                        serviceController.Start();
                        break;
                    case ServiceControllerStatus.StopPending:
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped, WaitTimeOut);
                        goto case ServiceControllerStatus.Stopped;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(serviceController.Status));
                }

                serviceController.WaitForStatus(ServiceControllerStatus.Running, WaitTimeOut);
            }
        }

        public static ServiceState GetServiceState(string serviceName)
        {
            if (!ServiceIsInstalled(serviceName))
            {
                return ServiceState.NotFound;
            }

            using (var serviceController = new ServiceController(serviceName))
            {
                return ConvertStatusToServiceState(serviceController.Status);
            }
        }

        private static bool ServiceIsInstalled(string serviceName)
        {
            var services = ServiceController.GetServices(Environment.MachineName);
            return services.Any(s => s.ServiceName == serviceName);
        }

        private static ServiceState ConvertStatusToServiceState(ServiceControllerStatus serviceStatus)
        {
            switch (serviceStatus)
            {
                case ServiceControllerStatus.ContinuePending:
                    return ServiceState.ContinuePending;
                case ServiceControllerStatus.Paused:
                    return ServiceState.Paused;
                case ServiceControllerStatus.PausePending:
                    return ServiceState.PausePending;
                case ServiceControllerStatus.Running:
                    return ServiceState.Running;
                case ServiceControllerStatus.StartPending:
                    return ServiceState.StartPending;
                case ServiceControllerStatus.Stopped:
                    return ServiceState.Stopped;
                case ServiceControllerStatus.StopPending:
                    return ServiceState.StopPending;
                default:
                    return ServiceState.Unknown;
            }
        }
    }
}