using System.Net.Sockets;
using log4net;

namespace ChatNetworking.utils
{
    public abstract class AbsConcurrentServer : AbstractServer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AbsConcurrentServer));

        protected AbsConcurrentServer(int port) : base(port)
        {
            Logger.Debug("Concurrent AbstractServer initialized");
        }

        // Overriding the base class method to provide concurrent behavior
        protected override void ProcessRequest(TcpClient client)
        {
            // 1. Create the worker thread via the abstract factory method
            Thread tw = CreateWorker(client);
            
            // 2. Start the thread
            tw.Start();
        }

        // Abstract method to be implemented by ChatJsonConcurrentServer
        protected abstract Thread CreateWorker(TcpClient client);
    }
}