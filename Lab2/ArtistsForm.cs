using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lab2.service;
using Org.Example.Domain;

namespace Lab2
{
    public class ArtistsForm : Form
    {
        private readonly ArtistService _artistService;
        private readonly SpectacleService _spectacleService;
        private readonly ArtistSpectacleService _artistSpectacleService;
        private Artist? _selectedArtist;

        private TextBox _textBoxNume;
        private DataGridView _dataGridViewArtists;
        private Button _buttonAdd;
        private Button _buttonDelete;
        private Button _buttonAssignToSpectacle;
        private DateTimePicker _dateTimePickerFilter;
        private Button _buttonSearchByDate;
        private DataGridView _dataGridViewArtistsByDate;
        private Label _statusLabel;

        public ArtistsForm(ArtistService artistService, SpectacleService spectacleService, ArtistSpectacleService artistSpectacleService)
        {
            _artistService = artistService;
            _spectacleService = spectacleService;
            _artistSpectacleService = artistSpectacleService;

            InitializeUI();
            LoadData();
        }

        private void InitializeUI()
        {
            Text = "Gestiune Artisti";
            ClientSize = new Size(920, 720);
            StartPosition = FormStartPosition.CenterScreen;

            var labelNume = new Label { Text = "Nume:", Location = new Point(15, 15), AutoSize = true };
            _textBoxNume = new TextBox { Location = new Point(70, 10), Width = 400 };

            _buttonAdd = new Button { Text = "Adauga/Actualizeaza", Location = new Point(480, 10), Width = 150 };
            _buttonAdd.Click += ButtonAdd_Click;

            _dataGridViewArtists = new DataGridView
            {
                Location = new Point(15, 50),
                Width = 890,
                Height = 240,
                AllowUserToAddRows = false,
                ReadOnly = true,
                ColumnHeadersVisible = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            _dataGridViewArtists.Columns.Add("IdCol", "ID");
            _dataGridViewArtists.Columns.Add("NumeCol", "Nume");
            _dataGridViewArtists.SelectionChanged += DataGridViewArtists_SelectionChanged;

            _buttonDelete = new Button { Text = "Sterge", Location = new Point(15, 300), Width = 100 };
            _buttonDelete.Click += ButtonDelete_Click;

            _buttonAssignToSpectacle = new Button { Text = "Asigneaza la Spectacol", Location = new Point(130, 300), Width = 170 };
            _buttonAssignToSpectacle.Click += ButtonAssignToSpectacle_Click;
            _buttonAssignToSpectacle.Enabled = false;

            var filterLabel = new Label { Text = "Arata artisti pentru ziua:", Location = new Point(15, 345), AutoSize = true };
            _dateTimePickerFilter = new DateTimePicker
            {
                Location = new Point(170, 340),
                Width = 170,
                Format = DateTimePickerFormat.Short
            };

            _buttonSearchByDate = new Button
            {
                Text = "Cauta",
                Location = new Point(350, 340),
                Width = 100
            };
            _buttonSearchByDate.Click += ButtonSearchByDate_Click;

            _dataGridViewArtistsByDate = new DataGridView
            {
                Location = new Point(15, 380),
                Width = 890,
                Height = 260,
                AllowUserToAddRows = false,
                ReadOnly = true,
                ColumnHeadersVisible = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _dataGridViewArtistsByDate.Columns.Add("ArtistCol", "Artist");
            _dataGridViewArtistsByDate.Columns.Add("SpectacolCol", "Spectacol");
            _dataGridViewArtistsByDate.Columns.Add("LocatieCol", "Locatie");
            _dataGridViewArtistsByDate.Columns.Add("OraCol", "Ora inceperii");
            _dataGridViewArtistsByDate.Columns.Add("LocuriCol", "Locuri disponibile");

            _statusLabel = new Label { Location = new Point(15, 655), Width = 890, ForeColor = Color.DarkGreen, Height = 40 };

            Controls.Add(labelNume);
            Controls.Add(_textBoxNume);
            Controls.Add(_buttonAdd);
            Controls.Add(_dataGridViewArtists);
            Controls.Add(_buttonDelete);
            Controls.Add(_buttonAssignToSpectacle);
            Controls.Add(filterLabel);
            Controls.Add(_dateTimePickerFilter);
            Controls.Add(_buttonSearchByDate);
            Controls.Add(_dataGridViewArtistsByDate);
            Controls.Add(_statusLabel);
        }

        private void LoadData()
        {
            _dataGridViewArtists.Rows.Clear();
            try
            {
                var artists = _artistService.GetAllArtists();
                foreach (var artist in artists)
                {
                    var rowIndex = _dataGridViewArtists.Rows.Add(artist.Id, artist.Name);
                    _dataGridViewArtists.Rows[rowIndex].Tag = artist;
                }
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = $"Eroare la incarcare: {ex.Message}";
            }

            _textBoxNume.Clear();
            _selectedArtist = null;
        }

        private void DataGridViewArtists_SelectionChanged(object? sender, EventArgs e)
        {
            if (_dataGridViewArtists.SelectedRows.Count > 0)
            {
                var row = _dataGridViewArtists.SelectedRows[0];
                _selectedArtist = (Artist?)row.Tag;
                _textBoxNume.Text = _selectedArtist?.Name ?? string.Empty;
                _buttonAssignToSpectacle.Enabled = true;
            }
            else
            {
                _selectedArtist = null;
                _buttonAssignToSpectacle.Enabled = false;
            }
        }

        private void ButtonAdd_Click(object? sender, EventArgs e)
        {
            var nume = _textBoxNume.Text.Trim();

            if (string.IsNullOrWhiteSpace(nume))
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = "Introduceti numele artistului!";
                return;
            }

            try
            {
                if (_selectedArtist != null)
                {
                    _selectedArtist.Name = nume;
                    _artistService.UpdateArtist(_selectedArtist);
                    _statusLabel.ForeColor = Color.DarkGreen;
                    _statusLabel.Text = "Artist actualizat!";
                }
                else
                {
                    _artistService.AddArtist(nume);
                    _statusLabel.ForeColor = Color.DarkGreen;
                    _statusLabel.Text = "Artist adaugat!";
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
            if (_selectedArtist == null)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = "Selectati un artist pentru a sterge!";
                return;
            }

            try
            {
                _artistService.DeleteArtist(_selectedArtist.Id);
                _statusLabel.ForeColor = Color.DarkGreen;
                _statusLabel.Text = "Artist sters!";
                LoadData();
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = $"Eroare: {ex.Message}";
            }
        }

        private void ButtonAssignToSpectacle_Click(object? sender, EventArgs e)
        {
            if (_selectedArtist == null)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = "Selectati un artist!";
                return;
            }

            using (var assignForm = new ArtistSpectacleForm(_artistService, _spectacleService, _artistSpectacleService, _selectedArtist))
            {
                assignForm.ShowDialog();
            }
        }

        private void ButtonSearchByDate_Click(object? sender, EventArgs e)
        {
            LoadArtistsByDate(_dateTimePickerFilter.Value.Date);
        }

        private void LoadArtistsByDate(DateTime selectedDate)
        {
            _dataGridViewArtistsByDate.Rows.Clear();

            try
            {
                var artistsById = _artistService.GetAllArtists().ToDictionary(a => a.Id, a => a);
                var spectaclesById = _spectacleService
                    .GetAllSpectacles()
                    .Where(s => s.Id.HasValue && s.StartDate.Date == selectedDate)
                    .ToDictionary(s => s.Id!.Value, s => s);

                var relations = _artistSpectacleService.GetAllRelations();
                foreach (var relation in relations)
                {
                    if (!artistsById.TryGetValue(relation.ArtistId, out var artist))
                    {
                        continue;
                    }

                    if (!spectaclesById.TryGetValue(relation.SpectacleId, out var spectacle))
                    {
                        continue;
                    }

                    _dataGridViewArtistsByDate.Rows.Add(
                        artist.Name,
                        spectacle.Name,
                        spectacle.Location,
                        spectacle.StartDate.ToString("HH:mm"),
                        _spectacleService.GetAvailableSeats(spectacle.Id!.Value)
                    );
                }

                _statusLabel.ForeColor = Color.DarkGreen;
                _statusLabel.Text = $"Rezultate pentru {selectedDate:dd.MM.yyyy}: {_dataGridViewArtistsByDate.Rows.Count} inregistrari.";
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = $"Eroare la cautare dupa zi: {ex.Message}";
            }
        }
    }
}
