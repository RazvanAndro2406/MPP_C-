using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        public void LoadData()
        {
            if (_servicesFacade == null) return;

            // In WPF, we don't need PropertyValueFactory. 
            // The XAML Binding handles it.
            var artists = _servicesFacade.GetAllArtists();
            
            _artistList.Clear();
            foreach (var artist in artists)
            {
                _artistList.Add(artist);
            }
        }

        private void HandleAdauga(object sender, RoutedEventArgs e)
        {
            string nume = TextFieldNume.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(nume))
            {
                MessageBox.Show("Introduceti numele artistului.", "Date invalide");
                return;
            }

            if (_selectedArtist != null)
            {
                _selectedArtist.Name = nume;
                _servicesFacade?.UpdateArtist(_selectedArtist);
                _selectedArtist = null;
            }
            else
            {
                _servicesFacade?.AddArtist(nume);
            }

            TextFieldNume.Clear();
            // In a real app, LoadData is called via the EventBus notification
            LoadData(); 
            _eventBus?.PublishMany(DomainUiEventBus.EventType.ArtistsChanged, 
                                   DomainUiEventBus.EventType.ArtistSpectaclesChanged);
        }

        private void HandleSterge(object sender, RoutedEventArgs e)
        {
            if (_selectedArtist == null)
            {
                MessageBox.Show("Selectati un artist pentru a sterge.", "Selectie lipsa");
                return;
            }

            _servicesFacade?.DeleteArtist(_selectedArtist.Id);
            TextFieldNume.Clear();
            _selectedArtist = null;
            LoadData();
            _eventBus?.PublishMany(DomainUiEventBus.EventType.ArtistsChanged, 
                                   DomainUiEventBus.EventType.ArtistSpectaclesChanged);
        }

        private void HandleAdaugaLaSpectacol(object sender, RoutedEventArgs e)
        {
            if (_selectedArtist == null) return;

            // Open the sub-window (Equivalent to FXMLLoader)
            ArtistSpectacleWindow subWin = new ArtistSpectacleWindow();
            subWin.SetServices(_servicesFacade!, _eventBus!);
            subWin.SetSelectedArtist(_selectedArtist);
            subWin.ShowDialog(); // Equivalent to showAndWait()
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

        // --- IDomainUiListener Implementation ---
        public void OnDomainEvent(DomainUiEventBus.EventType type)
        {
            if (type == DomainUiEventBus.EventType.ArtistsChanged)
            {
                // Platform.runLater replacement
                Application.Current.Dispatcher.Invoke(LoadData);
            }
        }
    }
}