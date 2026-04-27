using System;
using System.Threading.Tasks; // Required for Task
using System.Windows;
using ChatClient;
using log4net;
using Ticketing.Model.Domain;
using Ticketing.Services;

namespace Ticketing.Client.Gui
{
    public partial class LoginWindow : Window
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LoginWindow));
        private IServicesFacade? _servicesFacade;

        public LoginWindow()
        {
            InitializeComponent();
        }

        public void SetServicesFacade(IServicesFacade servicesFacade)
        {
            _servicesFacade = servicesFacade;
        }

        // 1. Mark the event handler as 'async'
        private async void HandleLogin(object sender, RoutedEventArgs e)
        {
            if (_servicesFacade == null) return;

            string username = TextFieldUsername.Text;
            string password = PasswordField.Password; 

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User user = new User(username, password);

            try
            {
                // 2. Disable UI so user doesn't spam the login button
                this.IsEnabled = false; 
                Logger.Info($"Attempting login for user: {username}");

                // 3. Prepare the next window
                MainWindow mainWin = new MainWindow();
                mainWin.SetAllServices(_servicesFacade, user);

                // 4. RUN ON BACKGROUND THREAD
                // await Task.Run prevents the UI thread from freezing while waiting for the server
                await Task.Run(() => 
                {
                    _servicesFacade.Login(user, mainWin);
                });

                // 5. Success! Switch windows (this part runs back on the UI thread)
                mainWin.Title = $"Main Menu - {user.Id}";
                mainWin.Closing += (s, args) => mainWin.LogoutAndClose();
                
                mainWin.Show();
                this.Hide(); 
            }
            catch (ChatException ex)
            {
                Logger.Warn($"Login failed: {ex.Message}");
                MessageBox.Show(ex.Message, "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.IsEnabled = true; // Re-enable if failed
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error during login", ex);
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.IsEnabled = true; // Re-enable if failed
            }
        }

        private void HandleCancel(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}