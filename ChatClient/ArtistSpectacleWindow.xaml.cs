using System;
using System.Collections.ObjectModel;
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

        private void LoadSpectacles()
        {
            if (_servicesFacade == null) return;

            var spectacles = _servicesFacade.GetAllSpectacles();
            _spectacleList.Clear();
            foreach (var s in spectacles)
            {
                _spectacleList.Add(s);
            }
        }

        public void OnDomainEvent(DomainUiEventBus.EventType type)
        {
            if (type == DomainUiEventBus.EventType.SpectaclesChanged)
            {
                // Equivalent to Platform.runLater
                Dispatcher.Invoke(LoadSpectacles);
            }
        }

        private void HandleSalveaza(object sender, RoutedEventArgs e)
        {
            var selectedSpectacle = ComboSpectacle.SelectedItem as Spectacle;

            if (_selectedArtist == null || selectedSpectacle == null)
            {
                MessageBox.Show("Selectati un artist si un spectacol.", "Date invalide");
                return;
            }

            try
            {
                _servicesFacade?.AddArtistSpectacle(_selectedArtist.Id, selectedSpectacle.Id);
                
                _eventBus?.Publish(DomainUiEventBus.EventType.ArtistSpectaclesChanged);
                
                this.Close(); // Close the window
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Operatie esuata");
            }
        }

        private void HandleAnuleaza(object sender, RoutedEventArgs e)
        {
            _eventBus?.Unsubscribe(this);
            this.Close();
        }
    }
}