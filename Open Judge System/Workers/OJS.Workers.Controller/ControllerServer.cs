namespace OJS.Workers.Controller
{
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    using log4net;

    public delegate void ClientConnectedEventHandler(object sender, ClientConnectedEventArgs e);

    public class ControllerServer
    {
        private readonly ILog logger;
        private readonly TcpListener tcpListener;
        private readonly Thread tcpListenerThread;

        public ControllerServer(int portNumber, ILog logger)
        {
            this.logger = logger;
            this.logger.Info("ControllerServer initializing...");

            this.tcpListener = new TcpListener(IPAddress.Any, portNumber);
            this.tcpListenerThread = new Thread(this.WaitForConnections);

            this.logger.Info("ControllerServer initialized.");
        }

        public event ClientConnectedEventHandler OnClientConnected;

        internal void Start()
        {
            this.logger.InfoFormat("ControllerServer starting on {0}...", this.tcpListener.LocalEndpoint);

            this.tcpListenerThread.Start();

            this.logger.Info("ControllerServer started.");
        }

        internal void Stop()
        {
            this.logger.Info("ControllerServer stopping...");

            this.tcpListenerThread.Abort();
            this.tcpListener.Stop();

            this.logger.Info("ControllerServer stopped.");
        }

        private void WaitForConnections()
        {
            while (true)
            {
                // blocks until a client has connected to the server
                var client = this.tcpListener.AcceptTcpClient();
                this.logger.InfoFormat("Client {0} connected!", client.Client.RemoteEndPoint);
                this.OnClientConnected(this, new ClientConnectedEventArgs(client));
            }
        }
    }
}
