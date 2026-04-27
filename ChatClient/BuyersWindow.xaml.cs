using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using log4net;
using Ticketing.Model.Domain;
using Ticketing.Services;

namespace ChatClient.Gui
{
    public partial class BuyersWindow : Window, DomainUiEventBus.IListener
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BuyersWindow));

        private IServicesFacade? _servicesFacade;
        private DomainUiEventBus? _eventBus;
        private Buyer? _selectedBuyer;
        private TicketSale? _selectedSale;

        private ObservableCollection<Buyer> _buyerList = new ObservableCollection<Buyer>();
        private ObservableCollection<TicketSale> _salesList = new ObservableCollection<TicketSale>();

        public BuyersWindow()
        {
            InitializeComponent();
            TableBuyers.ItemsSource = _buyerList;
            TableSales.ItemsSource = _salesList;

            this.Closed += (s, e) => _eventBus?.Unsubscribe(this);
        }

        public void SetServices(IServicesFacade servicesFacade, DomainUiEventBus eventBus)
        {
            _servicesFacade = servicesFacade;
            _eventBus = eventBus;
            _eventBus.Subscribe(this);
        }

        // Kept for backward compatibility
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
                // Fetch from server in background
                var buyers = await Task.Run(() => _servicesFacade.GetAllBuyers());
                
                // Store ID on UI thread to re-select after clear
                long? selectedId = _selectedBuyer?.Id;

                // Update Collection on UI thread
                _buyerList.Clear();
                foreach (var b in buyers) _buyerList.Add(b);

                // Re-select the buyer if they still exist
                if (selectedId.HasValue)
                {
                    var matched = _buyerList.FirstOrDefault(b => b.Id == selectedId.Value);
                    TableBuyers.SelectedItem = matched;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load buyers", ex);
                MessageBox.Show("Eroare incarcare Cumparatori", "Error");
            }
        }

        // REFACTOR: Network call moved to a background Task
        private async Task LoadSalesForSelectedBuyerAsync()
        {
            if (_selectedBuyer == null || _servicesFacade == null)
            {
                _salesList.Clear();
                return;
            }

            long buyerId = _selectedBuyer.Id;

            try
            {
                var sales = await Task.Run(() => _servicesFacade.GetTicketSalesByBuyer(buyerId));
                
                _salesList.Clear();
                foreach (var sale in sales) _salesList.Add(sale);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load sales for buyer", ex);
            }
        }

        // REFACTOR: Async Add
        private async void HandleAddBuyer(object sender, RoutedEventArgs e)
        {
            string name = TextFieldName.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Introdu numele cumparatorului.", "Date invalide");
                return;
            }

            this.IsEnabled = false;

            try
            {
                await Task.Run(() => 
                {
                    _servicesFacade?.GetOrCreateBuyer("", name); // Use empty string for email if missing
                });
                
                TextFieldName.Clear();
                _selectedBuyer = null;
                // No manual LoadData() or Publish() here - Server handles DomainDataChanged
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to add buyer", ex);
                MessageBox.Show("Eroare adaugare: " + ex.Message, "Error");
            }
            finally
            {
                this.IsEnabled = true;
            }
        }

        // REFACTOR: Async Delete
        private async void HandleDeleteBuyer(object sender, RoutedEventArgs e)
        {
            if (_selectedBuyer == null) return;

            long buyerId = _selectedBuyer.Id;
            this.IsEnabled = false;

            try
            {
                await Task.Run(() => 
                {
                    _servicesFacade?.DeleteBuyer(buyerId);
                });
                
                _selectedBuyer = null;
                _selectedSale = null;
                TextFieldName.Clear();
                _salesList.Clear();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to delete buyer", ex);
                MessageBox.Show("Eroare stergere: " + ex.Message, "Error");
            }
            finally
            {
                this.IsEnabled = true;
            }
        }

        // REFACTOR: Async Update
        private async void HandleIncreaseSeats(object sender, RoutedEventArgs e)
        {
            if (_selectedSale == null)
            {
                MessageBox.Show("Selecteaza un bilet din tabela de jos.", "Selectie lipsa");
                return;
            }

            if (!int.TryParse(TextExtraSeats.Text, out int extraSeats) || extraSeats <= 0)
            {
                MessageBox.Show("Locurile suplimentare trebuie sa fie pozitive.", "Date invalide");
                return;
            }

            long saleId = _selectedSale.Id;
            this.IsEnabled = false;

            try
            {
                await Task.Run(() => 
                {
                    _servicesFacade?.IncreaseTicketSeats(saleId, extraSeats);
                });
                
                TextExtraSeats.Clear();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to increase seats", ex);
                MessageBox.Show("Eroare actualizare: " + ex.Message, "Error");
            }
            finally
            {
                this.IsEnabled = true;
            }
        }

        private void TableBuyers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedBuyer = TableBuyers.SelectedItem as Buyer;
            if (_selectedBuyer != null)
            {
                TextFieldName.Text = _selectedBuyer.Name;
                _ = LoadSalesForSelectedBuyerAsync(); // Async fire-and-forget
            }
            else
            {
                _salesList.Clear();
                _selectedSale = null;
            }
        }

        private void TableSales_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSale = TableSales.SelectedItem as TicketSale;
        }

        // REFACTOR: Deadlock prevention on inbound server messages
        public void OnDomainEvent(DomainUiEventBus.EventType type)
        {
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                if (type == DomainUiEventBus.EventType.BuyersChanged)
                {
                    await LoadDataAsync();
                }
                else if (type == DomainUiEventBus.EventType.TicketSalesChanged || 
                         type == DomainUiEventBus.EventType.SpectaclesChanged)
                {
                    await LoadSalesForSelectedBuyerAsync();
                }
            }));
        }
    }
}