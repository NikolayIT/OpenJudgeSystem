namespace OJS.Common.Helpers
{
    using System;
    using System.Configuration.Install;
    using System.Linq;
    using System.ServiceProcess;

    public static class ServicesHelper
    {
        private const int WaitTimeInMilliseconds = 10000;

        public static void InstallAndStart(string serviceName, string filePath)
        {
            var installer = new AssemblyInstaller(filePath, null) { UseNewContext = true };
            installer.Install(null);
            installer.Commit(null);

            if (!ServiceIsInstalled(serviceName))
            {
                throw new ArgumentException("Unable to install service.");
            }

            StartService(serviceName);
        }

        public static void StartService(string serviceName)
        {
            var waitTimeOut = TimeSpan.FromMilliseconds(WaitTimeInMilliseconds);
            using (var serviceController = new ServiceController(serviceName))
            {
                if (serviceController.Status.Equals(ServiceControllerStatus.Running))
                {
                    return;
                }

                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running, waitTimeOut);
            }
        }

        public static bool ServiceIsInstalled(string serviceName)
        {
            var services = ServiceController.GetServices(Environment.MachineName);
            return services.Any(s => s.ServiceName == serviceName);
        }

        public static ServiceControllerStatus GetServiceStatus(string servicename)
        {
            using (var serviceController = new ServiceController(servicename))
            {
                return serviceController.Status;
            }
        }
    }
}