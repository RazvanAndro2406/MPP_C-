using System;
using System.Collections.Generic;
using System.IO;
using ChatNetworking.utils;
using log4net;
using log4net.Config;
using Ticketing.Persistence.Repository;
using Ticketing.Server.Service;
using Ticketing.Service;
using Ticketing.Services;

namespace server
{
    public class StartJsonServer
    {
        private static int _defaultPort = 55555;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(StartJsonServer));

        public static void Main(string[] args)
        {
            Console.WriteLine("DEBUG: Main method started!");
            
            try {
                XmlConfigurator.Configure(new FileInfo("log4net.config"));
                Console.WriteLine("DEBUG: Log4net configured.");
            } catch (Exception ex) {
                Console.WriteLine("DEBUG: Log4net failed: " + ex.Message);
            }
            
            IDictionary<string, string> serverProps = LoadProperties("resources/chatserver.properties");

            if (serverProps.Count == 0)
            {
                
                Logger.Error("Cannot find or load chatserver.properties");
                Console.WriteLine("FATAL ERROR: Properties file NOT FOUND or EMPTY!");
                Console.WriteLine("Current Directory: " + Directory.GetCurrentDirectory());
                Console.WriteLine("Press ENTER to close...");
                Console.ReadLine();
                return;
            }
           
            // 1. Extract Port
            int port = _defaultPort;
            if (serverProps.TryGetValue("server.port", out string portValue)) // Renamed to portValue
            {
                if (!int.TryParse(portValue, out int p))
                {
                    port = _defaultPort;
                }
                else
                {
                    port = p;
                }
            }

            
            // 2. Initialize Repositories (DECLARE OUTSIDE THE IF)
            IUserRepository userRepo = null;
            IMessageRepository messRepo = null;
            IArtistRepository artistRepo = null;
            ISpectacleRepository spectacleRepo = null;
            ITicketRepository ticketRepo = null;
            IBuyerRepository buyerRepo = null;
            ITicketSaleRepository ticketSaleRepo = null;
            IArtistSpectacleRepository artistSpectacleRepo = null;

            Console.WriteLine("Still ok0!");
            if (serverProps.TryGetValue("db.url", out string connectionString)) 
            {
                userRepo = new UserRepository(connectionString);
                messRepo = new MessageRepository(connectionString);
                artistRepo = new ArtistRepository(connectionString);
                spectacleRepo = new SpectacleRepository(connectionString);
                ticketRepo = new TicketRepository(connectionString);
                buyerRepo = new BuyerRepository(connectionString);
                ticketSaleRepo = new TicketSaleRepository(connectionString);
                artistSpectacleRepo = new ArtistSpectacleRepository(connectionString);
            }
            else 
            {
                Console.WriteLine("--- PROPERTY ERROR ---");
                Console.WriteLine("Could not find 'jdbc.url'. Here are the keys I DID find:");
                foreach (var key in serverProps.Keys)
                {
                    Console.WriteLine($" -> [{key}]");
                }
                Console.WriteLine("-----------------------");
                Logger.Error("jdbc.url not found in properties file!");
                Console.ReadLine(); // Wait so you can read the list
                return;
            }

            Logger.Info($"Starting server on port: {port}");

            
            
            // 3. Initialize Services (Business Logic)
            // Now these variables are visible because they were declared at the top
            IChatServices chatServerImpl = new ChatServicesImpl(userRepo, messRepo);
            IArtistService artistService = new ArtistServiceImpl(artistRepo);
            ISpectacleService spectacleService = new SpectacleServiceImpl(spectacleRepo);
            ITicketService ticketService = new TicketServiceImpl(ticketRepo);
            IBuyerService buyerService = new BuyerServiceImpl(buyerRepo);
            ITicketSaleService ticketSaleService = new TicketSaleServiceImpl(ticketSaleRepo, buyerRepo, spectacleRepo);
            IArtistSpectacleService artistSpectacleService = new ArtistSpectacleServiceImpl(artistSpectacleRepo);

            // 4. Wrap everything in a ServicesFacade (Matches Java style)
            IServicesFacade servicesFacade = new ServicesFacadeImpl(
                artistService,
                artistSpectacleService,
                buyerService,
                chatServerImpl,
                spectacleService,
                ticketSaleService,
                ticketService
            );

                    // 5. Determine Port
                    int serverPort = _defaultPort;
                    if (serverProps.TryGetValue("chat.server.port", out string portStr))
                    {
                        if (!int.TryParse(portStr, out serverPort))
                        {
                            Logger.Error($"Wrong Port Number: {portStr}");
                            Logger.Debug($"Using default port: {_defaultPort}");
                            serverPort = _defaultPort;
                        }
                    }
                    
                    Logger.Debug($"Starting server on port: {serverPort}");

                    // 6. Start the Server
                    // Assuming AbstractServer and ChatJsonConcurrentServer are refactored in your Utils
                    AbstractServer server = new ChatJsonConcurrentServer(
                        serverPort,
                        servicesFacade
                    );

                    Console.WriteLine("Still ok1!");
                    try
                    {
                        Console.WriteLine("Still ok2!");
                        server.Start();
                        Console.WriteLine("Still ok3!");
                        Logger.Info("Server started successfully and is waiting for connections...");
                        Console.WriteLine("SERVER RUNNING. Press [ENTER] to shut down the server.");
                        
                        //This will make the server to wait until something is write in console
                        Console.ReadLine(); 
            
                        Logger.Info("Server shutting down...");
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Error starting the server", e);
                    }
                }

                /// <summary>
                /// Helper method to mirror Java's Properties.load() behavior
                /// </summary>
                private static IDictionary<string, string> LoadProperties(string fileName)
                {
                    var props = new Dictionary<string, string>();
                    try
                    {
                        if (File.Exists(fileName))
                        {
                            foreach (var line in File.ReadAllLines(fileName))
                            {
                                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                                var parts = line.Split('=', 2);
                                if (parts.Length == 2) props[parts[0].Trim()] = parts[1].Trim();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error reading {fileName}: {ex.Message}");
                    }
                    return props;
                }
            }
}