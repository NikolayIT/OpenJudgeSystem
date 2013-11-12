namespace OJS.Workers.LocalWorker
{
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.ServiceProcess;

    [RunInstaller(true)]
    public class LocalWorkerServiceInstaller : Installer
    {
        public LocalWorkerServiceInstaller()
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
                DisplayName = "OJS Local Worker Service",
                ServiceName = "OJS Local Worker Service",
                Description =
                    "Executes processes in a sandboxed environment. Processes are executed on the current machine.",
            };

            this.Installers.AddRange(new Installer[] { serviceProcessInstaller, serviceInstaller });
        }
    }
}
