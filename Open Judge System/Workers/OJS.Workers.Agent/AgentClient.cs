namespace OJS.Workers.Agent
{
    using System;
    using System.IO;
    using System.Net.Sockets;

    using log4net;

    public class AgentClient : IDisposable
    {
        private readonly ILog logger;
        private readonly TcpClient tcpClient;

        private readonly string controllerAddress;
        private readonly int controllerPort;

        public AgentClient(string controllerAddress, int controllerPort, ILog logger)
        {
            this.logger = logger;
            this.controllerAddress = controllerAddress;
            this.controllerPort = controllerPort;

            this.tcpClient = new TcpClient();
        }

        public Stream Stream => this.tcpClient.GetStream();

        public void Start()
        {
            this.logger.InfoFormat("AgentClient starting with controller located on {0}:{1}...", this.controllerAddress, this.controllerPort);
            try
            {
                this.tcpClient.Connect(this.controllerAddress, this.controllerPort);
            }
            catch (SocketException exception)
            {
                this.logger.FatalFormat("Unable to connect to the controller. Caught SocketException: {0}", exception);
                throw;
            }
        }

        public void Stop()
        {
            this.tcpClient.Close();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Stop();
            }
        }
    }
}
