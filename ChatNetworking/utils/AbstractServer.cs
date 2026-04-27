using System.Net;
using System.Net.Sockets;
using log4net;

namespace ChatNetworking.utils
{
    public abstract class AbstractServer
    {
        private readonly int _port;
        private TcpListener? _server;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AbstractServer));

        protected AbstractServer(int port)
        {
            _port = port;
        }

        public virtual void Start()
        {
            try
            {
                // IPAddress.Any allows connections on all network interfaces
                _server = new TcpListener(IPAddress.Any, _port);
                _server.Start();

                while (true)
                {
                    Logger.Info("Waiting for clients ...");
                    
                    // Java: Socket client = server.accept();
                    TcpClient client = _server.AcceptTcpClient();
                    
                    Logger.Info("Client connected ...");
                    ProcessRequest(client);
                }
            }
            catch (Exception e)
            {
                throw new ServerException("Starting server error ", e);
            }
            finally
            {
                Stop();
            }
        }

        // Abstract method to be implemented by ConcurrentServer/SerialServer
        protected abstract void ProcessRequest(TcpClient client);

        public virtual void Stop()
        {
            if (_server == null)
            {
                return;
            }

            try
            {
                _server.Stop();
            }
            catch (Exception e)
            {
                throw new ServerException("Closing server error ", e);
            }
        }
    }
}