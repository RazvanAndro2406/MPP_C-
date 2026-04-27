using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using log4net;
using log4net.Config;
using Ticketing.Services;
using ChatNetworking.JsonProtocol;
using Ticketing.Client.Gui; // Ensure this matches your ported proxy namespace

namespace ChatClient
{
    /// <summary>
    /// The entry point of the WPF Application.
    /// Replaces the start() method from JavaFX.
    /// </summary>
    public partial class App : Application
    {
        
        private static readonly ILog Logger = LogManager.GetLogger(typeof(App));
        
        private const int DefaultChatPort = 55555;
        private const string DefaultServer = "localhost";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Initialize Logging
            XmlConfigurator.Configure();
            Logger.Info("Application starting...");

            // 2. Load Configuration from .properties file
            // Path: Resources/chatclient.properties (relative to the executable)
            var clientProps = LoadProperties("Resources/chatclient.properties");

            string serverIp = clientProps.ContainsKey("chat.server.host") 
                ? clientProps["chat.server.host"] 
                : DefaultServer;

            int serverPort = DefaultChatPort;
            if (clientProps.ContainsKey("chat.server.port"))
            {
                if (!int.TryParse(clientProps["chat.server.port"], out serverPort))
                {
                    Logger.Warn($"Invalid port in properties: {clientProps["chat.server.port"]}. Using default.");
                }
            }

            try
            {
                // 3. Setup Networking Layer (Proxy and Facade)
                Logger.Info($"Connecting to {serverIp}:{serverPort}");
                
                var sharedNetwork = new ChatServicesJsonProxy(serverIp, serverPort);
                IServicesFacade facade = new ServicesFacadeJsonProxy(sharedNetwork);

                // 4. Initialize and Show the Login Window
                LoginWindow loginWin = new LoginWindow();
                
                // Inject the facade into the "Controller" (LoginWindow)
                loginWin.SetServicesFacade(facade);
                
                loginWin.Title = "MPP Chat - Login";
                
                // Set this as the main window so the app closes when it closes (until logged in)
                this.MainWindow = loginWin;
                loginWin.Show();
            }
            catch (Exception ex)
            {
                Logger.Fatal("Failed to initialize networking facade.", ex);
                MessageBox.Show("Could not connect to the server. Please check your connection and try again.", 
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Exit the app if we can't even get to the login screen
                Shutdown();
            }
        }

        /// <summary>
        /// Helper to read Java-style .properties files (key=value)
        /// </summary>
        private Dictionary<string, string> LoadProperties(string fileName)
        {
            var props = new Dictionary<string, string>();
            try
            {
                if (File.Exists(fileName))
                {
                    foreach (var line in File.ReadAllLines(fileName))
                    {
                        string trimmed = line.Trim();
                        // Ignore empty lines and comments
                        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith("!"))
                            continue;

                        int index = trimmed.IndexOf('=');
                        if (index > 0)
                        {
                            string key = trimmed.Substring(0, index).Trim();
                            string value = trimmed.Substring(index + 1).Trim();
                            props[key] = value;
                        }
                    }
                }
                else
                {
                    Logger.Warn($"Properties file '{fileName}' not found. Using defaults.");
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