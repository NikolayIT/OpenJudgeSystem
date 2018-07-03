namespace OJS.Common.Helpers
{
    using System;
    using System.Configuration.Install;
    using System.Linq;
    using System.ServiceProcess;

    public static class ServicesHelper
    {
        private const int MaxWaitTimeInMilliseconds = 10000;

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
                throw new ArgumentException("Unable to install service.");
            }
        }

        public static void StartService(string serviceName)
        {
            using (var serviceController = new ServiceController(serviceName))
            {
                if (serviceController.Status.Equals(ServiceControllerStatus.Running))
                {
                    return;
                }

                var waitTimeOut = TimeSpan.FromMilliseconds(MaxWaitTimeInMilliseconds);

                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running, waitTimeOut);
            }
        }

        public static ServiceControllerStatus? GetServiceStatus(string servicename)
        {
            if (!ServiceIsInstalled(servicename))
            {
                return null;
            }

            using (var serviceController = new ServiceController(servicename))
            {
                return serviceController.Status;
            }
        }

        private static bool ServiceIsInstalled(string serviceName)
        {
            var services = ServiceController.GetServices(Environment.MachineName);
            return services.Any(s => s.ServiceName == serviceName);
        }
    }
}