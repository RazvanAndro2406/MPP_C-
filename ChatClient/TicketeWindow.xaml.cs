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
    public partial class TicketeWindow : Window, DomainUiEventBus.IListener
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TicketeWindow));

        private IServicesFacade? _servicesFacade;
        private DomainUiEventBus? _eventBus;
        private TicketSale? _selectedSale;
        
        private ObservableCollection<TicketSale> _ticketSalesList = new ObservableCollection<TicketSale>();
        private Dictionary<long, Spectacle> _spectaclesById = new Dictionary<long, Spectacle>();

        public TicketeWindow()
        {
            InitializeComponent();
            TableTickete.ItemsSource = _ticketSalesList;
            
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
                // 1. Load Spectacles for the ComboBox
                var spectacles = _servicesFacade.GetAllSpectacles();
                _spectaclesById.Clear();
                ComboBoxSpectacole.Items.Clear();
                
                foreach (var s in spectacles)
                {
                    _spectaclesById[s.Id] = s;
                    ComboBoxSpectacole.Items.Add($"{s.Id} - {s.Name}");
                }

                // 2. Load all Ticket Sales
                var sales = _servicesFacade.GetAllTicketSales();
                _ticketSalesList.Clear();
                foreach (var sale in sales)
                {
                    // Logic to ensure SpectacleName is populated for the Grid
                    if (string.IsNullOrEmpty(sale.SpectacleName) && _spectaclesById.ContainsKey(sale.SpectacleId))
                    {
                        sale.SpectacleName = _spectaclesById[sale.SpectacleId].Name;
                    }
                    _ticketSalesList.Add(sale);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load ticket data", ex);
                MessageBox.Show("Eroare incarcare date.", "Error");
            }
        }

        private void HandleAdauga(object sender, RoutedEventArgs e)
        {
            string buyerEmail = TextFieldBuyerEmail.Text?.Trim() ?? "";
            string? selectedSpecText = ComboBoxSpectacole.SelectedItem as string;

            if (string.IsNullOrEmpty(buyerEmail) || string.IsNullOrEmpty(selectedSpecText) || 
                !int.TryParse(TextLocuri.Text, out int seats) || seats <= 0)
            {
                MessageBox.Show("Completați toate câmpurile valid.", "Date invalide");
                return;
            }

            try
            {
                // Extract ID from "ID - Name"
                long spectacleId = long.Parse(selectedSpecText.Split(" - ")[0]);

                _servicesFacade?.SellTicket(spectacleId, buyerEmail, seats);

                ClearFields();
                _eventBus?.PublishMany(
                    DomainUiEventBus.EventType.TicketSalesChanged,
                    DomainUiEventBus.EventType.SpectaclesChanged,
                    DomainUiEventBus.EventType.BuyersChanged
                );
                
                MessageBox.Show($"Vânzare înregistrată pentru: {buyerEmail}", "Succes");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Eroare operație");
            }
        }

        private void HandleSterge(object sender, RoutedEventArgs e)
        {
            if (_selectedSale == null)
            {
                MessageBox.Show("Selectati o vanzare pentru marire locuri.", "Selectie lipsa");
                return;
            }

            if (!int.TryParse(TextLocuri.Text, out int extraSeats) || extraSeats <= 0)
            {
                MessageBox.Show("Introduceti un numar valid de locuri.", "Date invalide");
                return;
            }

            try
            {
                _servicesFacade?.IncreaseTicketSeats(_selectedSale.Id, extraSeats);
                ClearFields();
                _selectedSale = null;
                _eventBus?.PublishMany(DomainUiEventBus.EventType.TicketSalesChanged, 
                                       DomainUiEventBus.EventType.SpectaclesChanged);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Eroare");
            }
        }

        private void TableTickete_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSale = TableTickete.SelectedItem as TicketSale;
            if (_selectedSale != null)
            {
                TextFieldBuyerEmail.Text = _selectedSale.BuyerName; // Or Email if you have it
                TextLocuri.Text = _selectedSale.Seats.ToString();
                
                // Select the matching spectacle in ComboBox
                foreach (string item in ComboBoxSpectacole.Items)
                {
                    if (item.StartsWith(_selectedSale.SpectacleId + " - "))
                    {
                        ComboBoxSpectacole.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void ClearFields()
        {
            TextFieldBuyerEmail.Clear();
            ComboBoxSpectacole.SelectedIndex = -1;
            TextLocuri.Text = "1";
        }

        public void OnDomainEvent(DomainUiEventBus.EventType type)
        {
            if (type == DomainUiEventBus.EventType.TicketSalesChanged || 
                type == DomainUiEventBus.EventType.SpectaclesChanged)
            {
                Dispatcher.Invoke(LoadData);
            }
        }
    }
}