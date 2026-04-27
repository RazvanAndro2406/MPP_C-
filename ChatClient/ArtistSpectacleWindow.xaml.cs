using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Ticketing.Model.Domain;
using Ticketing.Services;

namespace ChatClient
{
    public partial class ArtistSpectacleWindow : Window, DomainUiEventBus.IListener
    {
        private IServicesFacade? _servicesFacade;
        private DomainUiEventBus? _eventBus;
        private Artist? _selectedArtist;

        private ObservableCollection<Spectacle> _spectacleList = new ObservableCollection<Spectacle>();

        public ArtistSpectacleWindow()
        {
            InitializeComponent();
            ComboSpectacle.ItemsSource = _spectacleList;
            
            // Clean up subscriptions automatically when the window closes
            this.Closed += (s, e) => _eventBus?.Unsubscribe(this);
        }

        public void SetServices(IServicesFacade servicesFacade, DomainUiEventBus eventBus)
        {
            _servicesFacade = servicesFacade;
            _eventBus = eventBus;
            _eventBus.Subscribe(this);
            LoadSpectacles();
        }

        public void SetSelectedArtist(Artist artist)
        {
            _selectedArtist = artist;
            LabelArtist.Text = $"Artist selectat: {artist.Name}";
        }

        public void LoadSpectacles()
        {
            _ = LoadSpectaclesAsync();
        }

        // REFACTOR: Network call moved to a background Task
        private async Task LoadSpectaclesAsync()
        {
            if (_servicesFacade == null) return;

            try
            {
                var spectacles = await Task.Run(() => _servicesFacade.GetAllSpectacles());
                
                _spectacleList.Clear();
                foreach (var s in spectacles)
                {
                    _spectacleList.Add(s);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la incarcarea spectacolelor: " + ex.Message);
            }
        }

        // REFACTOR: Deadlock prevention on inbound server messages
        public void OnDomainEvent(DomainUiEventBus.EventType type)
        {
            if (type == DomainUiEventBus.EventType.SpectaclesChanged)
            {
                // Fire and forget 
                Dispatcher.BeginInvoke(new Action(async () => await LoadSpectaclesAsync()));
            }
        }

        // REFACTOR: Async Save
        private async void HandleSalveaza(object sender, RoutedEventArgs e)
        {
            var selectedSpectacle = ComboSpectacle.SelectedItem as Spectacle;

            if (_selectedArtist == null || selectedSpectacle == null)
            {
                MessageBox.Show("Selectati un artist si un spectacol.", "Date invalide");
                return;
            }

            // Extract IDs on the UI thread before jumping to the background Task
            long artistId = _selectedArtist.Id;
            long spectacleId = selectedSpectacle.Id;

            this.IsEnabled = false;

            try
            {
                await Task.Run(() =>
                {
                    _servicesFacade?.AddArtistSpectacle(artistId, spectacleId);
                });
                
                // Note: No manual EventBus Publish here! Server sends DomainDataChanged.
                this.Close(); // Success! Close the popup.
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Operatie esuata");
                this.IsEnabled = true; // Only re-enable the UI if we failed and the window stays open
            }
        }

        private void HandleAnuleaza(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}