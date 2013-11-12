namespace OJS.Workers.Agent
{
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.ServiceProcess;

    [RunInstaller(true)]
    public class AgentServiceInstaller : Installer
    {
        public AgentServiceInstaller()
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
                                           DisplayName = "OJS Agent Service",
                                           ServiceName = "OJS Agent Service",
                                           Description =
                                               "Executes processes in a sandboxed environment. Tasks are received by a controller and executed by this service.",
                                       };

            this.Installers.AddRange(new Installer[] { serviceProcessInstaller, serviceInstaller });
        }
    }
}
