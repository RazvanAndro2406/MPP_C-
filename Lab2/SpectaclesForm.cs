using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lab2.domain;
using Lab2.service;

namespace Lab2
{
    public class SpectaclesForm : Form
    {
        private SpectacleService _spectacleService;
        private Spectacle? _selectedSpectacle;

        private TextBox _textBoxTitlu;
        private DateTimePicker _dateTimePicker;
        private NumericUpDown _numericDurata;
        private NumericUpDown _numericCapacitate;
        private TextBox _textBoxLocatie;
        private NumericUpDown _numericBuyerId;
        private TextBox _textBoxCumparator;
        private NumericUpDown _numericLocuriDorite;
        private DataGridView _dataGridViewSpectacles;
        private Button _buttonAdd;
        private Button _buttonDelete;
        private Button _buttonSellTickets;
        private Label _statusLabel;
        private System.Windows.Forms.Timer _refreshTimer;

        public SpectaclesForm(SpectacleService spectacleService)
        {
            _spectacleService = spectacleService;
            InitializeUI();
            LoadData();
        }

        private void InitializeUI()
        {
            Text = "Gestiune Spectacole";
            ClientSize = new Size(980, 520);
            StartPosition = FormStartPosition.CenterScreen;

            // Input fields
            var y = 10;
            const int labelWidth = 80;
            const int controlWidth = 150;

            var labelTitlu = new Label { Text = "Titlu:", Location = new Point(15, y), Width = labelWidth };
            _textBoxTitlu = new TextBox { Location = new Point(100, y), Width = controlWidth };

            y += 30;
            var labelData = new Label { Text = "Data:", Location = new Point(15, y), Width = labelWidth };
            _dateTimePicker = new DateTimePicker { Location = new Point(100, y), Width = controlWidth };

            y += 30;
            var labelDurata = new Label { Text = "Durata (min):", Location = new Point(15, y), Width = labelWidth };
            _numericDurata = new NumericUpDown { Location = new Point(100, y), Width = controlWidth, Minimum = 1, Maximum = 500000, Value = 2 };

            y += 30;
            var labelCapacitate = new Label { Text = "Capacitate:", Location = new Point(15, y), Width = labelWidth };
            _numericCapacitate = new NumericUpDown { Location = new Point(100, y), Width = controlWidth, Minimum = 1, Maximum = 1000000, Value = 100 };

            y += 30;
            var labelLocatie = new Label { Text = "Locatie:", Location = new Point(15, y), Width = labelWidth };
            _textBoxLocatie = new TextBox { Location = new Point(100, y), Width = controlWidth };

            y += 30;
            _buttonAdd = new Button { Text = "Adauga/Actualizeaza", Location = new Point(100, y), Width = 150 };
            _buttonAdd.Click += ButtonAdd_Click;

            var labelBuyerId = new Label { Text = "ID cumparator:", Location = new Point(300, 10), Width = 90 };
            _numericBuyerId = new NumericUpDown
            {
                Location = new Point(395, 10),
                Width = 180,
                Minimum = 0,
                Maximum = 1000000000,
                Value = 0
            };

            var labelCumparator = new Label { Text = "Cumparator:", Location = new Point(300, 40), Width = 90 };
            _textBoxCumparator = new TextBox { Location = new Point(395, 40), Width = 180 };

            var labelLocuri = new Label { Text = "Locuri:", Location = new Point(300, 70), Width = 90 };
            _numericLocuriDorite = new NumericUpDown
            {
                Location = new Point(395, 70),
                Width = 180,
                Minimum = 1,
                Maximum = 100000,
                Value = 1
            };

            _buttonSellTickets = new Button
            {
                Text = "Vinde bilete",
                Location = new Point(395, 100),
                Width = 180
            };
            _buttonSellTickets.Click += ButtonSellTickets_Click;

            // Table
            _dataGridViewSpectacles = new DataGridView
            {
                Location = new Point(15, y + 40),
                Width = 940,
                Height = 300,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersVisible = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            _dataGridViewSpectacles.Columns.Add("IdCol", "ID");
            _dataGridViewSpectacles.Columns.Add("TitluCol", "Titlu");
            _dataGridViewSpectacles.Columns.Add("DataCol", "Data");
            _dataGridViewSpectacles.Columns.Add("DurataCol", "Durata (min)");
            _dataGridViewSpectacles.Columns.Add("CapacitateCol", "Capacitate");
            _dataGridViewSpectacles.Columns.Add("DisponibileCol", "Locuri disponibile");
            _dataGridViewSpectacles.Columns.Add("LocatieCol", "Locatie");
            _dataGridViewSpectacles.SelectionChanged += DataGridViewSpectacles_SelectionChanged;

            // Action buttons
            _buttonDelete = new Button { Text = "Sterge", Location = new Point(15, y + 350), Width = 100 };
            _buttonDelete.Click += ButtonDelete_Click;

            _statusLabel = new Label { Location = new Point(15, y + 385), Width = 940, ForeColor = Color.DarkGreen };

            _refreshTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            _refreshTimer.Tick += (sender, e) => LoadData(true);
            _refreshTimer.Start();

            // Add controls
            Controls.Add(labelTitlu);
            Controls.Add(_textBoxTitlu);
            Controls.Add(labelData);
            Controls.Add(_dateTimePicker);
            Controls.Add(labelDurata);
            Controls.Add(_numericDurata);
            Controls.Add(labelCapacitate);
            Controls.Add(_numericCapacitate);
            Controls.Add(labelLocatie);
            Controls.Add(_textBoxLocatie);
            Controls.Add(_buttonAdd);
            Controls.Add(labelBuyerId);
            Controls.Add(_numericBuyerId);
            Controls.Add(labelCumparator);
            Controls.Add(_textBoxCumparator);
            Controls.Add(labelLocuri);
            Controls.Add(_numericLocuriDorite);
            Controls.Add(_buttonSellTickets);
            Controls.Add(_dataGridViewSpectacles);
            Controls.Add(_buttonDelete);
            Controls.Add(_statusLabel);
        }

        private void LoadData(bool preserveSelection = false)
        {
            long? selectedId = null;
            if (preserveSelection && _dataGridViewSpectacles.SelectedRows.Count > 0)
            {
                selectedId = (_dataGridViewSpectacles.SelectedRows[0].Tag as Spectacle)?.Id;
            }

            _dataGridViewSpectacles.Rows.Clear();
            try
            {
                var spectacles = _spectacleService.GetAllSpectacles();
                foreach (var spectacle in spectacles)
                {
                    var availableSeats = spectacle.Id.HasValue
                        ? _spectacleService.GetAvailableSeats(spectacle.Id.Value)
                        : 0;

                    int rowIndex = _dataGridViewSpectacles.Rows.Add(
                        spectacle.Id,
                        spectacle.Name ?? string.Empty,
                        spectacle.StartDate.ToString("dd.MM.yyyy HH:mm"),
                        spectacle.Duration,
                        spectacle.Capacity,
                        availableSeats,
                        spectacle.Location ?? string.Empty
                    );
                    _dataGridViewSpectacles.Rows[rowIndex].Tag = spectacle;

                    if (availableSeats <= 0)
                    {
                        _dataGridViewSpectacles.Rows[rowIndex].DefaultCellStyle.BackColor = Color.MistyRose;
                        _dataGridViewSpectacles.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                    }

                    if (selectedId.HasValue && spectacle.Id == selectedId.Value)
                    {
                        _dataGridViewSpectacles.Rows[rowIndex].Selected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = $"Eroare la incarcare: {ex.Message}";
            }

            if (!preserveSelection)
            {
                ClearFields();
                _selectedSpectacle = null;
            }
        }

        private void DataGridViewSpectacles_SelectionChanged(object? sender, EventArgs e)
        {
            if (_dataGridViewSpectacles.SelectedRows.Count > 0)
            {
                var row = _dataGridViewSpectacles.SelectedRows[0];
                _selectedSpectacle = (Spectacle?)row.Tag;
                if (_selectedSpectacle != null)
                {
                    _textBoxTitlu.Text = _selectedSpectacle.Name ?? "";
                    _dateTimePicker.Value = _selectedSpectacle.StartDate;
                    _numericDurata.Value = _selectedSpectacle.Duration;
                    _numericCapacitate.Value = _selectedSpectacle.Capacity;
                    _textBoxLocatie.Text = _selectedSpectacle.Location ?? "";
                }
            }
            else
            {
                _selectedSpectacle = null;
            }
        }

        private void ButtonAdd_Click(object? sender, EventArgs e)
        {
            var titlu = _textBoxTitlu.Text.Trim();
            var locatie = _textBoxLocatie.Text.Trim();

            if (string.IsNullOrWhiteSpace(titlu) || string.IsNullOrWhiteSpace(locatie))
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = "Completati toate campurile!";
                return;
            }

            try
            {
                var durata = (int)_numericDurata.Value;
                var capacitate = (int)_numericCapacitate.Value;
                var data = _dateTimePicker.Value;

                if (_selectedSpectacle != null)
                {
                    _selectedSpectacle.Name = titlu;
                    _selectedSpectacle.StartDate = data;
                    _selectedSpectacle.Duration = durata;
                    _selectedSpectacle.Capacity = capacitate;
                    _selectedSpectacle.Location = locatie;
                    _spectacleService.UpdateSpectacle(_selectedSpectacle);
                    _statusLabel.ForeColor = Color.DarkGreen;
                    _statusLabel.Text = "Spectacol actualizat!";
                }
                else
                {
                    _spectacleService.AddSpectacle(titlu, data, durata, capacitate, locatie);
                    _statusLabel.ForeColor = Color.DarkGreen;
                    _statusLabel.Text = "Spectacol adaugat!";
                }

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
            if (_selectedSpectacle == null)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = "Selectati un spectacol pentru a sterge!";
                return;
            }

            try
            {
                _spectacleService.DeleteSpectacle(_selectedSpectacle.Id ?? 0);
                _statusLabel.ForeColor = Color.DarkGreen;
                _statusLabel.Text = "Spectacol sters!";
                LoadData();
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = $"Eroare: {ex.Message}";
            }
        }

        private void ButtonSellTickets_Click(object? sender, EventArgs e)
        {
            if (_selectedSpectacle?.Id == null)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = "Selectati un spectacol pentru vanzare!";
                return;
            }

            var buyerName = _textBoxCumparator.Text.Trim();
            var buyerId = (long)_numericBuyerId.Value;
            var requestedSeats = (int)_numericLocuriDorite.Value;

            if (buyerId <= 0)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = "Introduceti ID-ul cumparatorului!";
                return;
            }

            if (string.IsNullOrWhiteSpace(buyerName))
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = "Introduceti numele cumparatorului!";
                return;
            }

            try
            {
                _spectacleService.SellTickets(_selectedSpectacle.Id.Value, buyerId, buyerName, requestedSeats);
                _statusLabel.ForeColor = Color.DarkGreen;
                _statusLabel.Text = $"Vanzare reusita: {requestedSeats} loc(uri) pentru {buyerName} (ID {buyerId}).";
                LoadData(true);
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = $"Eroare la vanzare: {ex.Message}";
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            base.OnFormClosed(e);
        }

        private void ClearFields()
        {
            _textBoxTitlu.Clear();
            _dateTimePicker.Value = DateTime.Now;
            _numericDurata.Value = 2;
            _numericCapacitate.Value = 100;
            _textBoxLocatie.Clear();
            _numericBuyerId.Value = 0;
            _textBoxCumparator.Clear();
            _numericLocuriDorite.Value = 1;
        }
    }
}

