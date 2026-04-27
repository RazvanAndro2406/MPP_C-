using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        public void LoadData()
        {
            if (_servicesFacade == null) return;

            try
            {
                var buyers = _servicesFacade.GetAllBuyers();
                _buyerList.Clear();
                foreach (var b in buyers) _buyerList.Add(b);

                // Re-select the buyer if they still exist (prevents losing selection on update)
                if (_selectedBuyer != null)
                {
                    var matched = _buyerList.FirstOrDefault(b => b.Id == _selectedBuyer.Id);
                    TableBuyers.SelectedItem = matched;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load buyers", ex);
                MessageBox.Show("Eroare incarcare Cumparatori", "Error");
            }
        }

        private void LoadSalesForSelectedBuyer()
        {
            if (_selectedBuyer == null || _servicesFacade == null)
            {
                _salesList.Clear();
                return;
            }

            var sales = _servicesFacade.GetTicketSalesByBuyer(_selectedBuyer.Id);
            _salesList.Clear();
            foreach (var sale in sales) _salesList.Add(sale);
        }

        private void HandleAddBuyer(object sender, RoutedEventArgs e)
        {
            string name = TextFieldName.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Introdu numele cumparatorului.", "Date invalide");
                return;
            }

            try
            {
                _servicesFacade?.GetOrCreateBuyer(null, name); // null email for generic manual add
                TextFieldName.Clear();
                _selectedBuyer = null;
                LoadData();
                _eventBus?.Publish(DomainUiEventBus.EventType.BuyersChanged);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to add buyer", ex);
                MessageBox.Show("Eroare adaugare", "Error");
            }
        }

        private void HandleDeleteBuyer(object sender, RoutedEventArgs e)
        {
            if (_selectedBuyer == null) return;

            try
            {
                _servicesFacade?.DeleteBuyer(_selectedBuyer.Id);
                _selectedBuyer = null;
                _selectedSale = null;
                TextFieldName.Clear();
                _salesList.Clear();
                LoadData();
                _eventBus?.Publish(DomainUiEventBus.EventType.BuyersChanged);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to delete buyer", ex);
                MessageBox.Show("Eroare stergere", "Error");
            }
        }

        private void HandleIncreaseSeats(object sender, RoutedEventArgs e)
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

            try
            {
                _servicesFacade?.IncreaseTicketSeats(_selectedSale.Id, extraSeats);
                LoadSalesForSelectedBuyer();
                _eventBus?.PublishMany(DomainUiEventBus.EventType.TicketSalesChanged, 
                                       DomainUiEventBus.EventType.SpectaclesChanged);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to increase seats", ex);
                MessageBox.Show("Eroare actualizare", "Error");
            }
        }

        private void TableBuyers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedBuyer = TableBuyers.SelectedItem as Buyer;
            if (_selectedBuyer != null)
            {
                TextFieldName.Text = _selectedBuyer.Name;
                LoadSalesForSelectedBuyer();
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

        public void OnDomainEvent(DomainUiEventBus.EventType type)
        {
            // Use Dispatcher to ensure UI updates happen on the main thread
            Dispatcher.Invoke(() =>
            {
                if (type == DomainUiEventBus.EventType.BuyersChanged)
                {
                    LoadData();
                }
                else if (type == DomainUiEventBus.EventType.TicketSalesChanged || 
                         type == DomainUiEventBus.EventType.SpectaclesChanged)
                {
                    LoadSalesForSelectedBuyer();
                }
            });
        }
    }
}