using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ChatClient.Gui;
using log4net;
using Ticketing.Model.Domain;
using Ticketing.Services;

namespace ChatClient
{
    public partial class MainWindow : Window, IChatObserver
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MainWindow));

        private IServicesFacade? _servicesFacade;
        private User? _currentUser;
        private readonly DomainUiEventBus _eventBus = new DomainUiEventBus();

        public MainWindow()
        {
            InitializeComponent();
        }

        public void SetAllServices(IServicesFacade servicesFacade, User currentUser)
        {
            _servicesFacade = servicesFacade;
            _currentUser = currentUser;

            WelcomeLabel.Text = $"Bun venit, {currentUser.Id}!";
        }

        private void HandleArtisti(object sender, RoutedEventArgs e)
        {
            try
            {
                ArtistiWindow win = new ArtistiWindow();
                win.SetServices(_servicesFacade, _eventBus);
                win.Title = "Gestiune Artisti";
                // Note: If LoadData connects to the DB, it should be made async in ArtistiWindow
                win.LoadData(); 
                win.Show();
            }
            catch (Exception ex)
            {
                Logger.Error("Eroare la deschiderea ferestrei Artisti", ex);
                MessageBox.Show("Nu pot deschide fereastra Artisti.", "UI Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleSpectacole(object sender, RoutedEventArgs e)
        {
            try
            {
                SpectacoleWindow win = new SpectacoleWindow();
                win.SetService(_servicesFacade, _eventBus);
                win.LoadData();
                win.Show();
            }
            catch (Exception ex)
            {
                Logger.Error("Eroare la deschiderea ferestrei Spectacole", ex);
                MessageBox.Show("Nu pot deschide fereastra Spectacole.", "UI Error");
            }
        }

        private void HandleTickete(object sender, RoutedEventArgs e)
        {
            try
            {
                TicketeWindow win = new TicketeWindow();
                win.SetServices(_servicesFacade, _eventBus);
                win.LoadData();
                win.Show();
            }
            catch (Exception ex)
            {
                Logger.Error("Eroare la deschiderea ferestrei Tickete", ex);
                MessageBox.Show($"Nu pot deschide fereastra Tickete. {ex.Message}", "UI Error");
            }
        }

        private void HandleCumparatori(object sender, RoutedEventArgs e)
        {
            try
            {
                BuyersWindow win = new BuyersWindow();
                win.SetServices(_servicesFacade, _eventBus);
                win.LoadData();
                win.Show();
            }
            catch (Exception ex)
            {
                Logger.Error("Eroare la deschiderea ferestrei Cumparatori", ex);
                MessageBox.Show("Nu pot deschide fereastra Cumparatori.", "UI Error");
            }
        }

        // REFACTOR 1: Make button handler async
        private async void HandleLogout(object sender, RoutedEventArgs e)
        {
            // Prevent multiple clicks while waiting for server
            this.IsEnabled = false; 
            await LogoutAsync();
            this.Close();
        }

        // REFACTOR 2: Push network operations to a background Task
        public async Task LogoutAsync()
        {
            if (_servicesFacade == null || _currentUser == null) return;

            try
            {
                await Task.Run(() =>
                {
                    _servicesFacade.Logout(_currentUser, this);
                });
            }
            catch (ChatException ex)
            {
                Logger.Warn("Logout failed", ex);
            }
        }

        // Kept for backward compatibility if you call it directly from Window_Closing events
        public void LogoutAndClose()
        {
            if (_servicesFacade == null || _currentUser == null) return;
            try { _servicesFacade.Logout(_currentUser, this); } catch { }
        }

        // --- IChatObserver Implementation ---

        public void MessageReceived(Message message)
        {
            Logger.Debug($"Message received: {message}");
        }

        public void FriendLoggedIn(User friend) { /* Logic if needed */ }
        public void FriendLoggedOut(User friend) { /* Logic if needed */ }

        public void DomainDataChanged(string entityType)
        {
            Logger.Debug($"Domain update notification: {entityType}");

            // REFACTOR 3: Fire-and-forget UI updates. 
            // The ReaderThread doesn't wait for this to finish anymore.
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                string type = entityType?.ToUpper() ?? "ALL";
                
                switch (type)
                {
                    case "ARTISTS":
                        _eventBus.Publish(DomainUiEventBus.EventType.ArtistsChanged);
                        break;
                    case "SPECTACLES":
                        _eventBus.Publish(DomainUiEventBus.EventType.SpectaclesChanged);
                        break;
                    case "TICKETS":
                        _eventBus.Publish(DomainUiEventBus.EventType.TicketsChanged);
                        break;
                    case "CUMPARATOR_CUMPARA_BILET":
                    case "SALES":
                        _eventBus.PublishMany(
                            DomainUiEventBus.EventType.TicketSalesChanged,
                            DomainUiEventBus.EventType.SpectaclesChanged
                        );
                        break;
                    default:
                        _eventBus.PublishAll(); 
                        break;
                }
            }));
        }
    }
}