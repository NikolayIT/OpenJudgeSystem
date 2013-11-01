namespace OJS.Workers.Controller
{
    using System.Configuration;
    using System.ServiceProcess;
    using System.Threading;

    using log4net;
    using System.Net.Sockets;
    using System;

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

        private void ServerOnClientConnected(TcpClient tcpClient, EventArgs e)
        {
            var communicator = new ControllerCommunicator(tcpClient.GetStream(), logger);
            // communicator.
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
    }
}
