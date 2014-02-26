namespace OJS.Workers.Controller
{
    using System;
    using System.Net.Sockets;

    public class ClientConnectedEventArgs : EventArgs
    {
        public ClientConnectedEventArgs(TcpClient client)
        {
            this.Client = client;
        }

        public TcpClient Client { get; private set; }
    }
}
