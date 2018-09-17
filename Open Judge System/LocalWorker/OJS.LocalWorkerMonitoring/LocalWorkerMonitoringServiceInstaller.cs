namespace OJS.LocalWorkerMonitoring
{
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.ServiceProcess;

    using OJS.Workers.Common;

    [RunInstaller(true)]
    public class LocalWorkerMonitoringServiceInstaller : Installer
    {
        public LocalWorkerMonitoringServiceInstaller()
        {
            var serviceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem,
                Password = null,
                Username = null
            };

            var serviceInstaller = new ServiceInstaller
            {
                StartType = ServiceStartMode.Automatic,
                DisplayName = Constants.LocalWorkerMonitoringServiceName,
                ServiceName = Constants.LocalWorkerMonitoringServiceName,
                Description =
                    $"Monitors the {Constants.LocalWorkerServiceName} and restarts it or sends email if not running"
            };

            this.Installers.AddRange(new Installer[] { serviceProcessInstaller, serviceInstaller });
        }
    }
}