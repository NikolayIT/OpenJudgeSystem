namespace OJS.Workers.Controller
{
    using System.IO;

    using log4net;

    public class ControllerCommunicator
    {
        private readonly ILog logger;
        private readonly Stream clientStream;

        public ControllerCommunicator(Stream clientStream, ILog logger)
        {
            this.logger = logger;
            this.logger.Info("ControllerCommunicator initializing...");

            this.clientStream = clientStream;

            this.logger.Info("ControllerCommunicator initialized.");
        }

        public void Start()
        {
            this.logger.Info("Start()");
        }

        public void Stop()
        {
            this.logger.Info("Stop()");
        }
    }
}

/*
class Program
{
    private static TcpListener tcplistener;

    static void Main()
    {
        tcplistener = new TcpListener(IPAddress.Any, 8888);
        tcplistener.Start();
        Console.WriteLine("Server started!");

        while (true)
        {
            // blocks until a client has connected to the server
            var client = tcplistener.AcceptTcpClient();

            // create a thread to handle communication with connected client
            var clientThread = new Thread(HandleClientComm);
            clientThread.Start(client);
        }
    }

    private static void HandleClientComm(object client)
    {
        var tcpClient = (TcpClient)client;
        Console.WriteLine("Client {0} connected!", tcpClient.Client.RemoteEndPoint);
        var clientStream = tcpClient.GetStream();

        while (true)
        {
            try
            {
                // blocks until a client sends a message
                // bytesRead = clientStream.Read(message, 0, 4096);
                var formatter = new BinaryFormatter();
                var obj = formatter.Deserialize(clientStream);
                Console.WriteLine(((Data)obj).Name);
            }
            catch
            {
                // a socket error has occurred
                break;
            }

            // message has successfully been received
            // var encoder = new ASCIIEncoding();
            // var bufferingMessage = encoder.GetString(message, 0, bytesRead);
            // Console.WriteLine("Client {0}: {1}", tcpClient.Client.RemoteEndPoint, bufferingMessage);
        }
        Console.WriteLine("Client {0} disconnected!", tcpClient.Client.RemoteEndPoint);
    }
}
 */