using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ticketing.Model.Domain;
using Ticketing.Services;

namespace ChatClient
{
    public partial class ArtistiWindow : Window, DomainUiEventBus.IListener
    {
        private IServicesFacade? _servicesFacade;
        private DomainUiEventBus? _eventBus;
        private Artist? _selectedArtist;
        
        // C# equivalent of ObservableList
        private ObservableCollection<Artist> _artistList = new ObservableCollection<Artist>();

        public ArtistiWindow()
        {
            InitializeComponent();
            TableArtisti.ItemsSource = _artistList;
            
            // Unsubscribe when window closes (onStageReady equivalent)
            this.Closed += (s, e) => _eventBus?.Unsubscribe(this);
        }

        public void SetServices(IServicesFacade servicesFacade, DomainUiEventBus eventBus)
        {
            _servicesFacade = servicesFacade;
            _eventBus = eventBus;
            _eventBus.Subscribe(this);
        }

        // Maintained for backward compatibility (e.g., when called from MainWindow)
        public void LoadData()
        {
            _ = LoadDataAsync();
        }

        // REFACTOR: Network call moved to a background Task
        public async Task LoadDataAsync()
        {
            if (_servicesFacade == null) return;

            try
            {
                // 1. Fetch from server on background thread
                var artists = await Task.Run(() => _servicesFacade.GetAllArtists());
                
                // 2. Update UI Collection on main thread
                _artistList.Clear();
                foreach (var artist in artists)
                {
                    _artistList.Add(artist);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la descarcarea artistilor: " + ex.Message);
            }
        }

        // REFACTOR: Async Add/Update
        private async void HandleAdauga(object sender, RoutedEventArgs e)
        {
            string nume = TextFieldNume.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(nume))
            {
                MessageBox.Show("Introduceti numele artistului.", "Date invalide");
                return;
            }

            // Extract variables needed for the background thread
            long? idToUpdate = _selectedArtist?.Id;

            // Disable UI so user doesn't spam requests
            this.IsEnabled = false;

            try
            {
                await Task.Run(() =>
                {
                    if (idToUpdate.HasValue)
                    {
                        // Create a clean, disconnected entity for thread-safety
                        Artist artistToUpdate = new Artist(nume) { Id = idToUpdate.Value };
                        _servicesFacade?.UpdateArtist(artistToUpdate);
                    }
                    else
                    {
                        _servicesFacade?.AddArtist(nume);
                    }
                });

                TextFieldNume.Clear();
                _selectedArtist = null;

                // Note: We DO NOT call LoadData() or EventBus.Publish() here anymore!
                // The server will dispatch "ARTISTS", triggering OnDomainEvent below automatically.
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la retea: " + ex.Message, "Eroare");
            }
            finally
            {
                this.IsEnabled = true;
            }
        }

        // REFACTOR: Async Delete
        private async void HandleSterge(object sender, RoutedEventArgs e)
        {
            if (_selectedArtist == null)
            {
                MessageBox.Show("Selectati un artist pentru a sterge.", "Selectie lipsa");
                return;
            }

            long artistId = _selectedArtist.Id;
            this.IsEnabled = false;

            try
            {
                await Task.Run(() =>
                {
                    _servicesFacade?.DeleteArtist(artistId);
                });

                TextFieldNume.Clear();
                _selectedArtist = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la stergere: " + ex.Message);
            }
            finally
            {
                this.IsEnabled = true;
            }
        }

        private void HandleAdaugaLaSpectacol(object sender, RoutedEventArgs e)
        {
            if (_selectedArtist == null) return;

            ArtistSpectacleWindow subWin = new ArtistSpectacleWindow();
            subWin.SetServices(_servicesFacade!, _eventBus!);
            subWin.SetSelectedArtist(_selectedArtist);
            subWin.ShowDialog(); 
        }

        private void TableArtisti_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedArtist = TableArtisti.SelectedItem as Artist;
            if (_selectedArtist != null)
            {
                TextFieldNume.Text = _selectedArtist.Name;
                ButtonAdaugaLaSpectacol.IsEnabled = true;
            }
            else
            {
                ButtonAdaugaLaSpectacol.IsEnabled = false;
            }
        }

        // REFACTOR: Deadlock prevention on inbound server messages
        public void OnDomainEvent(DomainUiEventBus.EventType type)
        {
            if (type == DomainUiEventBus.EventType.ArtistsChanged)
            {
                // Fire and forget - unblocks the network Reader Thread instantly
                Dispatcher.BeginInvoke(new Action(async () => await LoadDataAsync()));
            }
        }
    }
}