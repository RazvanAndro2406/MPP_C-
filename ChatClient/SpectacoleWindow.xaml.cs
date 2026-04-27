using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using log4net;
using Ticketing.Model.Domain;
using Ticketing.Services;

namespace ChatClient
{
    public partial class SpectacoleWindow : Window, DomainUiEventBus.IListener
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SpectacoleWindow));
        private IServicesFacade? _servicesFacade;
        private DomainUiEventBus? _eventBus;
        private Spectacle? _selectedSpectacle;
        
        // We use a Wrapper to show Sold/Available info in the table
        public class SpectacleViewItem : Spectacle 
        {
            public int Sold { get; set; }
            public int Available => Math.Max(0, Capacity - Sold);
            
            public SpectacleViewItem(Spectacle s, int sold) {
                this.Id = s.Id;
                this.Name = s.Name;
                this.Start_date = s.Start_date;
                this.Duration = s.Duration;
                this.Capacity = s.Capacity;
                this.Location = s.Location;
                this.Sold = sold;
            }
        }

        private ObservableCollection<SpectacleViewItem> _spectacleList = new ObservableCollection<SpectacleViewItem>();

        public SpectacoleWindow()
        {
            InitializeComponent();
            TableSpectacole.ItemsSource = _spectacleList;
            
            // Re-bind calculated columns manually since they are in the wrapper
            ((DataGridTextColumn)TableSpectacole.Columns[5]).Binding = new System.Windows.Data.Binding("Sold");
            ((DataGridTextColumn)TableSpectacole.Columns[6]).Binding = new System.Windows.Data.Binding("Available");
            
            this.Closed += (s, e) => _eventBus?.Unsubscribe(this);
        }

        public void SetService(IServicesFacade servicesFacade, DomainUiEventBus eventBus)
        {
            _servicesFacade = servicesFacade;
            _eventBus = eventBus;
            _eventBus.Subscribe(this);
        }

        public void LoadData()
        {
            if (_servicesFacade == null) return;

            var spectacles = _servicesFacade.GetAllSpectacles();
            _spectacleList.Clear();

            foreach (var s in spectacles)
            {
                int sold = _servicesFacade.GetSoldSeatsForSpectacle(s.Id);
                _spectacleList.Add(new SpectacleViewItem(s, sold));
            }
        }

        private void HandleAdauga(object sender, RoutedEventArgs e)
        {
            string titlu = TextFieldTitlu.Text?.Trim() ?? "";
            string locatie = TextFieldLocatie.Text?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(titlu) || string.IsNullOrEmpty(locatie) || !DatePickerData.SelectedDate.HasValue)
            {
                MessageBox.Show("Completati toate campurile.", "Date invalide");
                return;
            }

            DateTime data = DatePickerData.SelectedDate.Value.Add(DateTime.Now.TimeOfDay);
            int durata = int.Parse(TextDurata.Text);
            int capacitate = int.Parse(TextCapacitate.Text);

            if (_selectedSpectacle != null)
            {
                _selectedSpectacle.Name = titlu;
                _selectedSpectacle.Start_date = data;
                _selectedSpectacle.Duration = durata;
                _selectedSpectacle.Capacity = capacitate;
                _selectedSpectacle.Location = locatie;
                _servicesFacade?.UpdateSpectacle(_selectedSpectacle);
                _selectedSpectacle = null;
            }
            else
            {
                _servicesFacade?.AddSpectacle(titlu, data, durata, capacitate, locatie);
            }

            ClearFields();
            LoadData();
            _eventBus?.PublishMany(DomainUiEventBus.EventType.SpectaclesChanged, 
                                   DomainUiEventBus.EventType.TicketsChanged);
        }

        private void HandleVindeBilete(object sender, RoutedEventArgs e)
        {
            if (_selectedSpectacle == null) {
                MessageBox.Show("Selectați un spectacol din tabel.", "Selecție lipsă");
                return;
            }

            string email = TextFieldBuyerEmail.Text?.Trim() ?? "";
            if (!int.TryParse(TextLocuriVanzare.Text, out int seats) || seats <= 0 || string.IsNullOrEmpty(email)) {
                MessageBox.Show("Date invalide pentru vanzare.", "Eroare");
                return;
            }

            try {
                _servicesFacade?.SellTicket(_selectedSpectacle.Id, email, seats);
                MessageBox.Show($"Bilete vandute cu succes catre: {email}", "Succes");
                LoadData();
                _eventBus?.PublishMany(DomainUiEventBus.EventType.TicketSalesChanged, DomainUiEventBus.EventType.SpectaclesChanged);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Eroare vanzare");
            }
        }

        private void TableSpectacole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = TableSpectacole.SelectedItem as SpectacleViewItem;
            if (item != null)
            {
                _selectedSpectacle = item;
                TextFieldTitlu.Text = item.Name;
                DatePickerData.SelectedDate = item.Start_date;
                TextDurata.Text = item.Duration.ToString();
                TextCapacitate.Text = item.Capacity.ToString();
                TextFieldLocatie.Text = item.Location;
            }
        }

        private void HandleSterge(object sender, RoutedEventArgs e)
        {
            if (_selectedSpectacle != null)
            {
                _servicesFacade?.DeleteSpectacle(_selectedSpectacle.Id);
                LoadData();
                _eventBus?.Publish(DomainUiEventBus.EventType.SpectaclesChanged);
            }
        }

        private void ClearFields() {
            TextFieldTitlu.Clear();
            TextFieldLocatie.Clear();
            DatePickerData.SelectedDate = null;
        }

        public void OnDomainEvent(DomainUiEventBus.EventType type)
        {
            if (type == DomainUiEventBus.EventType.SpectaclesChanged || type == DomainUiEventBus.EventType.TicketSalesChanged)
            {
                Dispatcher.Invoke(LoadData);
            }
        }
    }
}