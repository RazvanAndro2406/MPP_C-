using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

        // To maintain compatibility with MainWindow calling win.LoadData()
        public void LoadData()
        {
            _ = LoadDataAsync();
        }

        // REFACTOR: Heavy network loop moved to background Task
        public async Task LoadDataAsync()
        {
            if (_servicesFacade == null) return;

            try
            {
                // Fetch all data on a background thread so UI doesn't freeze
                var backgroundList = await Task.Run(() =>
                {
                    var spectacles = _servicesFacade.GetAllSpectacles();
                    var results = new List<SpectacleViewItem>();

                    foreach (var s in spectacles)
                    {
                        int sold = _servicesFacade.GetSoldSeatsForSpectacle(s.Id);
                        results.Add(new SpectacleViewItem(s, sold));
                    }
                    return results;
                });

                // Update UI Collection back on the main thread
                _spectacleList.Clear();
                foreach (var item in backgroundList)
                {
                    _spectacleList.Add(item);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Eroare la incarcarea spectacolelor", ex);
            }
        }

        // REFACTOR: Async Add/Update
        private async void HandleAdauga(object sender, RoutedEventArgs e)
        {
            string titlu = TextFieldTitlu.Text?.Trim() ?? "";
            string locatie = TextFieldLocatie.Text?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(titlu) || string.IsNullOrEmpty(locatie) || !DatePickerData.SelectedDate.HasValue)
            {
                MessageBox.Show("Completati toate campurile.", "Date invalide");
                return;
            }

            // Extract values on UI thread before Task.Run
            DateTime data = DatePickerData.SelectedDate.Value.Add(DateTime.Now.TimeOfDay);
            int durata = int.Parse(TextDurata.Text);
            int capacitate = int.Parse(TextCapacitate.Text);
            long? idToUpdate = _selectedSpectacle?.Id;

            // Disable buttons to prevent spam clicks
            this.IsEnabled = false;

            try
            {
                await Task.Run(() =>
                {
                    if (idToUpdate.HasValue)
                    {
                        // Create a clean entity to send to the server (thread-safe)
                        Spectacle specToUpdate = new Spectacle(titlu, data, durata, capacitate, locatie) { Id = idToUpdate.Value };
                        _servicesFacade?.UpdateSpectacle(specToUpdate);
                    }
                    else
                    {
                        _servicesFacade?.AddSpectacle(titlu, data, durata, capacitate, locatie);
                    }
                });

                ClearFields();
                _selectedSpectacle = null;
                
                // NOTE: We don't call LoadData() or EventBus.Publish() here anymore! 
                // The Server will send DomainDataChanged, which triggers OnDomainEvent below.
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

        // REFACTOR: Async Sell Tickets
        private async void HandleVindeBilete(object sender, RoutedEventArgs e)
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

            long specId = _selectedSpectacle.Id;
            this.IsEnabled = false;

            try {
                await Task.Run(() => 
                {
                    _servicesFacade?.SellTicket(specId, email, seats);
                });
                
                MessageBox.Show($"Bilete vandute cu succes catre: {email}", "Succes");
                TextFieldBuyerEmail.Clear();
                TextLocuriVanzare.Clear();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Eroare vanzare");
            }
            finally {
                this.IsEnabled = true;
            }
        }

        // REFACTOR: Async Delete
        private async void HandleSterge(object sender, RoutedEventArgs e)
        {
            if (_selectedSpectacle == null) return;

            long specId = _selectedSpectacle.Id;
            this.IsEnabled = false;

            try
            {
                await Task.Run(() => 
                {
                    _servicesFacade?.DeleteSpectacle(specId);
                });
                ClearFields();
                _selectedSpectacle = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare stergere: " + ex.Message);
            }
            finally
            {
                this.IsEnabled = true;
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

        private void ClearFields() {
            TextFieldTitlu.Clear();
            TextFieldLocatie.Clear();
            TextDurata.Clear();
            TextCapacitate.Clear();
            DatePickerData.SelectedDate = null;
        }

        // REFACTOR: Prevent deadlocks on inbound network notifications
        public void OnDomainEvent(DomainUiEventBus.EventType type)
        {
            if (type == DomainUiEventBus.EventType.SpectaclesChanged || type == DomainUiEventBus.EventType.TicketSalesChanged)
            {
                // Fire and forget - lets the Reader Thread go right back to work!
                Dispatcher.BeginInvoke(new Action(async () => await LoadDataAsync()));
            }
        }
    }
}