using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lab2.domain;
using Lab2.service;

namespace Lab2
{
    public class BuyersForm : Form
    {
        private readonly BuyerService _buyerService;
        private readonly SpectacleService _spectacleService;
        private Buyer? _selectedBuyer;
        private TicketSale? _selectedTicketSale;

        private TextBox _textBoxName;
        private DataGridView _dataGridViewBuyers;
        private DataGridView _dataGridViewBuyerTickets;
        private Button _buttonAdd;
        private Button _buttonDelete;
        private NumericUpDown _numericExtraSeats;
        private Button _buttonIncreaseSeats;
        private Label _statusLabel;

        public BuyersForm(BuyerService buyerService, SpectacleService spectacleService)
        {
            _buyerService = buyerService;
            _spectacleService = spectacleService;
            InitializeUI();
            LoadData();
        }

        private void InitializeUI()
        {
            Text = "Gestiune Cumparatori";
            ClientSize = new Size(980, 610);
            StartPosition = FormStartPosition.CenterScreen;

            var labelName = new Label { Text = "Nume:", Location = new Point(15, 15), AutoSize = true };
            _textBoxName = new TextBox { Location = new Point(70, 10), Width = 400 };

            _buttonAdd = new Button { Text = "Adauga", Location = new Point(480, 10), Width = 100 };
            _buttonAdd.Click += ButtonAdd_Click;

            _dataGridViewBuyers = new DataGridView
            {
                Location = new Point(15, 50),
                Width = 950,
                Height = 220,
                AllowUserToAddRows = false,
                ReadOnly = true,
                ColumnHeadersVisible = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            _dataGridViewBuyers.Columns.Add("IdCol", "ID");
            _dataGridViewBuyers.Columns.Add("NumeCol", "Nume");
            _dataGridViewBuyers.SelectionChanged += DataGridViewBuyers_SelectionChanged;

            _buttonDelete = new Button { Text = "Sterge", Location = new Point(15, 280), Width = 100 };
            _buttonDelete.Click += ButtonDelete_Click;

            var ticketsLabel = new Label { Text = "Bilete cumparator selectat:", Location = new Point(15, 320), AutoSize = true };

            _dataGridViewBuyerTickets = new DataGridView
            {
                Location = new Point(15, 345),
                Width = 950,
                Height = 190,
                AllowUserToAddRows = false,
                ReadOnly = true,
                ColumnHeadersVisible = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            _dataGridViewBuyerTickets.Columns.Add("TicketIdCol", "ID bilet");
            _dataGridViewBuyerTickets.Columns.Add("SpectacolCol", "Spectacol");
            _dataGridViewBuyerTickets.Columns.Add("SoldAtCol", "Cumparat la");
            _dataGridViewBuyerTickets.Columns.Add("SeatsCol", "Locuri");
            _dataGridViewBuyerTickets.SelectionChanged += DataGridViewBuyerTickets_SelectionChanged;

            var labelExtraSeats = new Label { Text = "Locuri suplimentare:", Location = new Point(15, 545), AutoSize = true };
            _numericExtraSeats = new NumericUpDown
            {
                Location = new Point(190, 542),
                Width = 120,
                Minimum = 1,
                Maximum = 100000,
                Value = 1
            };

            _buttonIncreaseSeats = new Button { Text = "Mareste locurile", Location = new Point(325, 540), Width = 140 };
            _buttonIncreaseSeats.Click += ButtonIncreaseSeats_Click;

            _statusLabel = new Label { Location = new Point(480, 545), Width = 545, ForeColor = Color.DarkGreen, Height = 40 };

            Controls.Add(labelName);
            Controls.Add(_textBoxName);
            Controls.Add(_buttonAdd);
            Controls.Add(_dataGridViewBuyers);
            Controls.Add(_buttonDelete);
            Controls.Add(ticketsLabel);
            Controls.Add(_dataGridViewBuyerTickets);
            Controls.Add(labelExtraSeats);
            Controls.Add(_numericExtraSeats);
            Controls.Add(_buttonIncreaseSeats);
            Controls.Add(_statusLabel);
        }

        private void LoadData()
        {
            _dataGridViewBuyers.Rows.Clear();
            try
            {
                var buyers = _buyerService.GetAllBuyers();
                foreach (var buyer in buyers)
                {
                    var rowIndex = _dataGridViewBuyers.Rows.Add(buyer.Id, buyer.Name);
                    _dataGridViewBuyers.Rows[rowIndex].Tag = buyer;
                }

                _statusLabel.ForeColor = Color.DarkGreen;
                _statusLabel.Text = $"Total cumparatori: {buyers.Count}";
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = $"Eroare la incarcare: {ex.Message}";
            }

            _textBoxName.Clear();
            _selectedBuyer = null;
            _selectedTicketSale = null;
            _dataGridViewBuyerTickets.Rows.Clear();
        }

        private void DataGridViewBuyers_SelectionChanged(object? sender, EventArgs e)
        {
            if (_dataGridViewBuyers.SelectedRows.Count > 0)
            {
                var row = _dataGridViewBuyers.SelectedRows[0];
                _selectedBuyer = (Buyer?)row.Tag;
                _textBoxName.Text = _selectedBuyer?.Name ?? string.Empty;
                LoadBuyerTickets(_selectedBuyer?.Id ?? 0);
            }
            else
            {
                _selectedBuyer = null;
                _textBoxName.Clear();
                _selectedTicketSale = null;
                _dataGridViewBuyerTickets.Rows.Clear();
            }
        }

        private void ButtonAdd_Click(object? sender, EventArgs e)
        {
            var name = _textBoxName.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = "Introduceti numele cumparatorului!";
                return;
            }

            try
            {
                _buyerService.GetOrCreateBuyer(0, name);
                _statusLabel.ForeColor = Color.DarkGreen;
                _statusLabel.Text = "Cumparator adaugat!";
                LoadData();
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = $"Eroare: {ex.Message}";
            }
        }

        private void ButtonDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedBuyer == null)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = "Selectati un cumparator pentru a sterge!";
                return;
            }

            try
            {
                _buyerService.DeleteBuyer(_selectedBuyer.Id);
                _statusLabel.ForeColor = Color.DarkGreen;
                _statusLabel.Text = "Cumparator sters!";
                LoadData();
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = $"Eroare: {ex.Message}";
            }
        }

        private void LoadBuyerTickets(long buyerId)
        {
            _dataGridViewBuyerTickets.Rows.Clear();
            _selectedTicketSale = null;

            if (buyerId <= 0)
            {
                return;
            }

            try
            {
                var sales = _spectacleService.GetTicketSalesByBuyer(buyerId);
                foreach (var sale in sales)
                {
                    var rowIndex = _dataGridViewBuyerTickets.Rows.Add(
                        sale.Id,
                        sale.SpectacleName,
                        sale.SoldAt.ToString("dd.MM.yyyy HH:mm"),
                        sale.Seats
                    );
                    _dataGridViewBuyerTickets.Rows[rowIndex].Tag = sale;
                }
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = $"Eroare la incarcare bilete: {ex.Message}";
            }
        }

        private void DataGridViewBuyerTickets_SelectionChanged(object? sender, EventArgs e)
        {
            if (_dataGridViewBuyerTickets.SelectedRows.Count == 0)
            {
                _selectedTicketSale = null;
                return;
            }

            var row = _dataGridViewBuyerTickets.SelectedRows[0];
            _selectedTicketSale = (TicketSale?)row.Tag;
        }

        private void ButtonIncreaseSeats_Click(object? sender, EventArgs e)
        {
            if (_selectedBuyer == null)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = "Selectati un cumparator.";
                return;
            }

            DataGridViewRow? selectedRow;
            if (_dataGridViewBuyerTickets.SelectedRows.Count > 0)
            {
                selectedRow = _dataGridViewBuyerTickets.SelectedRows[0];
            }
            else
            {
                selectedRow = _dataGridViewBuyerTickets.CurrentRow;
            }

            var selectedSale = selectedRow?.Tag as TicketSale;
            if (selectedSale?.Id == null)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = "Selectati un bilet din tabelul de bilete.";
                return;
            }

            _selectedTicketSale = selectedSale;

            var extraSeats = (int)_numericExtraSeats.Value;
            try
            {
                _spectacleService.IncreaseTicketSeats(_selectedTicketSale.Id.Value, extraSeats);
                _statusLabel.ForeColor = Color.DarkGreen;
                _statusLabel.Text = $"Bilet actualizat cu +{extraSeats} loc(uri).";
                LoadBuyerTickets(_selectedBuyer.Id);
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = $"Eroare la actualizare bilet: {ex.Message}";
            }
        }
    }
}

