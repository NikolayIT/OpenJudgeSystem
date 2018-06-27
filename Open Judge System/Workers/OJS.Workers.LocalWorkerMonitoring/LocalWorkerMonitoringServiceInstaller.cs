namespace OJS.Workers.LocalWorkerMonitoring
{
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.ServiceProcess;

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
                DisplayName = "OJS Local Worker Monitoring Service",
                ServiceName = "OJS Local Worker Monitoring Service",
                Description = "Monitors the OJS Local Worker Service",
            };

            this.Installers.AddRange(new Installer[] { serviceProcessInstaller, serviceInstaller });
        }
    }
}