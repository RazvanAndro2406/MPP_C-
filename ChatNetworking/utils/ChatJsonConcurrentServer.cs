using System.Net.Sockets;
using ChatNetworking.JsonProtocol;
using log4net;
using Ticketing.Services;

namespace ChatNetworking.utils
{
    public class ChatJsonConcurrentServer : AbsConcurrentServer
    {
        private readonly IServicesFacade _servicesFacade;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChatJsonConcurrentServer));

        public ChatJsonConcurrentServer(int port, IServicesFacade servicesFacade) 
            : base(port)
        {
            _servicesFacade = servicesFacade;
            Logger.Info("Chat-ChatJsonConcurrentServer initialized");
        }

        /// <summary>
        /// This is the implementation of the abstract factory method from AbsConcurrentServer.
        /// It creates a worker and wraps its Run method in a new System.Threading.Thread.
        /// </summary>
        protected override Thread CreateWorker(TcpClient client)
        {
            // 1. Initialize the worker with the facade and the client connection
            // Note: We used 'ClientJsonWorker' in our previous C# port
            ChatClientJsonWorker worker = new ChatClientJsonWorker(_servicesFacade, client);
            // 2. Return a new thread pointing to the worker's Run method
            // Java: return new Thread(worker);
            // C#: You pass the method delegate directly
            return new Thread(new ThreadStart(worker.Run))
            {
                // Ensures the thread is a background thread so it doesn't 
                // prevent the process from exiting.
                IsBackground = true
            };
        }
    }
}