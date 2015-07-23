namespace OJS.Workers.Controller
{
    using System.Configuration;
    using System.ServiceProcess;
    using System.Threading;

    using log4net;

    public class ControllerService : ServiceBase
    {
        private const int DefaultPort = 8888;
        private static ILog logger;
        private readonly ControllerServer server;

        public ControllerService()
        {
            logger = LogManager.GetLogger("ControllerService");
            logger.Info("ControllerService initializing...");

            // Read port value
            int port;
            if (ConfigurationManager.AppSettings["ControllerPort"] != null)
            {
                if (!int.TryParse(ConfigurationManager.AppSettings["ControllerPort"], out port))
                {
                    port = DefaultPort;
                }
            }
            else
            {
                port = DefaultPort;
            }

            // Create server
            this.server = new ControllerServer(port, logger);
            this.server.OnClientConnected += this.ServerOnClientConnected;

            logger.Info("ControllerService initialized.");
        }

        protected override void OnStart(string[] args)
        {
            logger.Info("ControllerService starting...");

            this.server.Start();

            logger.Info("ControllerService started.");
        }

        protected override void OnStop()
        {
            logger.Info("ControllerService stopping...");

            this.server.Stop();
            Thread.Sleep(10 * 1000); // Wait 10 seconds for the server to close connections before exit

            logger.Info("ControllerService stopped.");
        }

        private void ServerOnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            var communicator = new ControllerCommunicator(e.Client.GetStream(), logger);
            //// communicator.
        }
    }
}
