namespace OJS.LocalWorker
{
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.ServiceProcess;

    using OJS.Workers.Common;

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
                DisplayName = Constants.LocalWorkerServiceName,
                ServiceName = Constants.LocalWorkerServiceName,
                Description =
                    "Evaluates submissions for the Open Judge System and executes processes in a sandboxed environment. Processes are executed on the current machine.",
            };

            this.Installers.AddRange(new Installer[] { serviceProcessInstaller, serviceInstaller });
        }
    }
}
