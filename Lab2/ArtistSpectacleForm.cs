using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lab2.domain;
using Lab2.service;
using Org.Example.Domain;

namespace Lab2
{
    public class ArtistSpectacleForm : Form
    {
        private ArtistService _artistService;
        private SpectacleService _spectacleService;
        private ArtistSpectacleService _artistSpectacleService;
        private Artist _selectedArtist;

        private Label _labelArtist;
        private ComboBox _comboBoxSpectacle;
        private Button _buttonSave;
        private Button _buttonCancel;
        private Label _statusLabel;

        public ArtistSpectacleForm(ArtistService artistService, SpectacleService spectacleService, ArtistSpectacleService artistSpectacleService, Artist artist)
        {
            _artistService = artistService;
            _spectacleService = spectacleService;
            _artistSpectacleService = artistSpectacleService;
            _selectedArtist = artist;

            InitializeUI();
            LoadSpectacles();
        }

        private void InitializeUI()
        {
            Text = "Asigneaza Artist la Spectacol";
            ClientSize = new Size(400, 180);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            _labelArtist = new Label
            {
                Text = $"Artist: {_selectedArtist.Name}",
                Location = new Point(15, 20),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            var labelSpectacle = new Label
            {
                Text = "Spectacol:",
                Location = new Point(15, 50),
                AutoSize = true
            };

            _comboBoxSpectacle = new ComboBox
            {
                Location = new Point(90, 45),
                Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _buttonSave = new Button
            {
                Text = "Salveaza",
                Location = new Point(90, 90),
                Width = 100
            };
            _buttonSave.Click += ButtonSave_Click;

            _buttonCancel = new Button
            {
                Text = "Anuleaza",
                Location = new Point(200, 90),
                Width = 100
            };
            _buttonCancel.Click += ButtonCancel_Click;

            _statusLabel = new Label
            {
                Location = new Point(15, 130),
                Width = 370,
                ForeColor = Color.DarkRed
            };

            Controls.Add(_labelArtist);
            Controls.Add(labelSpectacle);
            Controls.Add(_comboBoxSpectacle);
            Controls.Add(_buttonSave);
            Controls.Add(_buttonCancel);
            Controls.Add(_statusLabel);
        }

        private void LoadSpectacles()
        {
            try
            {
                var spectacles = _spectacleService.GetAllSpectacles();
                _comboBoxSpectacle.DataSource = spectacles;
                _comboBoxSpectacle.DisplayMember = "Name";
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"Eroare: {ex.Message}";
            }
        }

        private void ButtonSave_Click(object? sender, EventArgs e)
        {
            if (_comboBoxSpectacle.SelectedItem == null)
            {
                _statusLabel.Text = "Selectati un spectacol!";
                return;
            }

            try
            {
                var spectacle = (Spectacle)_comboBoxSpectacle.SelectedItem;
                _artistSpectacleService.AddRelation(_selectedArtist.Id, spectacle.Id ?? 0);
                _statusLabel.ForeColor = Color.DarkGreen;
                _statusLabel.Text = "Relatie adaugata cu succes!";
                System.Threading.Thread.Sleep(1000);
                Close();
            }
            catch (ArgumentException ex)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = $"Eroare: {ex.Message}";
            }
        }

        private void ButtonCancel_Click(object? sender, EventArgs e)
        {
            Close();
        }
    }
}

