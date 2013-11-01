namespace OJS.Workers.Agent
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.ServiceProcess;
    using System.Threading;
    using System.Threading.Tasks;

    using log4net;

    internal class AgentService : ServiceBase
    {
        private const int DefaultThreadsCount = 2;
        private static ILog logger;
        private readonly List<AgentClient> clients;

        public AgentService()
        {
            logger = LogManager.GetLogger("AgentService");
            logger.Info("AgentService initializing...");

            // TODO: Extract class ConfigurationManager
            // Read controller address
            if (ConfigurationManager.AppSettings["ControllerAddress"] == null)
            {
                logger.Fatal("ControllerAddress setting not found in App.config file!");
                throw new Exception("ControllerAddress setting not found in App.config file!");
            }

            var controllerAddress = ConfigurationManager.AppSettings["ControllerAddress"];

            // Read controller port
            if (ConfigurationManager.AppSettings["ControllerPort"] == null)
            {
                logger.Fatal("ControllerPort setting not found in App.config file!");
                throw new Exception("ControllerPort setting not found in App.config file!");
            }

            int controllerPort;
            if (!int.TryParse(ConfigurationManager.AppSettings["ControllerPort"], out controllerPort))
            {
                logger.Fatal("ControllerPort setting is not an integer!");
                throw new Exception("ControllerPort setting is not an integer!");
            }

            // Read threads count value
            int threadsCount;
            if (ConfigurationManager.AppSettings["ThreadsCount"] != null)
            {
                if (!int.TryParse(ConfigurationManager.AppSettings["ThreadsCount"], out threadsCount))
                {
                    threadsCount = DefaultThreadsCount;
                }
            }
            else
            {
                threadsCount = DefaultThreadsCount;
            }

            // Create clients
            this.clients = new List<AgentClient>();
            for (var i = 1; i <= threadsCount; i++)
            {
                var client = new AgentClient(controllerAddress, controllerPort, logger);
                this.clients.Add(client);
            }

            logger.Info("AgentService initialized.");
        }

        protected override void OnStart(string[] args)
        {
            logger.Info("AgentService starting...");

            foreach (var agentClient in this.clients)
            {
                var client = agentClient;
                Task.Run(() => StartCommunication(client));
            }

            logger.Info("AgentService started.");
        }

        protected override void OnStop()
        {
            logger.Info("AgentService stopping...");

            foreach (var agentClient in this.clients)
            {
                agentClient.Stop();
            }

            Thread.Sleep(10 * 1000); // Wait 10 seconds for the client to close connections before exit

            logger.Info("AgentService stopped.");
        }

        private static void StartCommunication(AgentClient client)
        {
            client.Start();
            var stream = client.Stream;
            var communicator = new AgentCommunicator(stream, new BinaryFormatter(), new CodeJobWorker(), FileCache.Instance, logger);
            communicator.StartCommunicationLoop();
        }
    }
}
