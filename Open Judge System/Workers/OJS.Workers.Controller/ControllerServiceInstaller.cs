namespace OJS.Workers.Controller
{
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.ServiceProcess;

    [RunInstaller(true)]
    public class ControllerServiceInstaller : Installer
    {
        public ControllerServiceInstaller()
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
                                           DisplayName = "OJS Controller Service",
                                           ServiceName = "OJS Controller Service",
                                           Description =
                                               "Remotely controls all agents that are connected to the controller. Manages the tasks between agents and act as a load-balancer between them.",
                                       };

            this.Installers.AddRange(new Installer[] { serviceProcessInstaller, serviceInstaller });
        }
    }
}
