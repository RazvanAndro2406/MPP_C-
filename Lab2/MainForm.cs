using System;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;
using Lab2.domain;
using Lab2.repository;
using Lab2.service;
using org.example.repository;

namespace Lab2
{
    public class MainForm : Form
    {
        private User _currentUser;
        private ArtistService? _artistService;
        private SpectacleService? _spectacleService;
        private ArtistSpectacleService? _artistSpectacleService;
        private BuyerService? _buyerService;

        public MainForm(User user)
        {
            _currentUser = user;
            InitializeServices();
            InitializeUI();
        }

        private void InitializeServices()
        {
            try
            {
                var connString = ConfigurationManager.ConnectionStrings["spectacoleDB"]?.ConnectionString;
                if (string.IsNullOrWhiteSpace(connString))
                {
                    MessageBox.Show("Conexiune DB ne-configurata!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var artistRepo = new ArtistRepository(connString);
                var spectacleRepo = new SpectacleRepository(connString);
                var artistSpectacleRepo = new ArtistSpectacleRepository(connString);
                var buyerRepo = new BuyerRepository(connString);

                _artistService = new ArtistService(artistRepo);
                _buyerService = new BuyerService(buyerRepo);
                _spectacleService = new SpectacleService(spectacleRepo, _buyerService);
                _artistSpectacleService = new ArtistSpectacleService(artistSpectacleRepo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la initializare servici: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeUI()
        {
            Text = "Aplicatie - Gestiune Spectacole";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(400, 320);

            var welcomeLabel = new Label
            {
                Text = $"Bine ai venit, {_currentUser.Username}!",
                AutoSize = true,
                Location = new Point(20, 20),
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            var buttonArtists = new Button
            {
                Text = "Gestiune Artisti",
                Location = new Point(50, 70),
                Width = 300,
                Height = 40,
                Font = new Font("Arial", 10)
            };
            buttonArtists.Click += (s, e) => OpenArtistsForm();

            var buttonSpectacles = new Button
            {
                Text = "Gestiune Spectacole",
                Location = new Point(50, 120),
                Width = 300,
                Height = 40,
                Font = new Font("Arial", 10)
            };
            buttonSpectacles.Click += (s, e) => OpenSpectaclesForm();

            var buttonBuyers = new Button
            {
                Text = "Gestiune Cumparatori",
                Location = new Point(50, 170),
                Width = 300,
                Height = 40,
                Font = new Font("Arial", 10)
            };
            buttonBuyers.Click += (s, e) => OpenBuyersForm();

            var buttonLogout = new Button
            {
                Text = "Logout",
                Location = new Point(50, 220),
                Width = 300,
                Height = 40,
                Font = new Font("Arial", 10)
            };
            buttonLogout.Click += (s, e) => Close();

            Controls.Add(welcomeLabel);
            Controls.Add(buttonArtists);
            Controls.Add(buttonSpectacles);
            Controls.Add(buttonBuyers);
            Controls.Add(buttonLogout);
        }

        private void OpenArtistsForm()
        {
            if (_artistService == null || _spectacleService == null || _artistSpectacleService == null)
            {
                MessageBox.Show("Serviciile nu sunt initializate!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var artistsForm = new ArtistsForm(_artistService, _spectacleService, _artistSpectacleService);
            artistsForm.ShowDialog();
        }

        private void OpenSpectaclesForm()
        {
            if (_spectacleService == null)
            {
                MessageBox.Show("Serviciile nu sunt initializate!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var spectaclesForm = new SpectaclesForm(_spectacleService);
            spectaclesForm.ShowDialog();
        }

        private void OpenBuyersForm()
        {
            if (_buyerService == null || _spectacleService == null)
            {
                MessageBox.Show("Serviciile nu sunt initializate!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var buyersForm = new BuyersForm(_buyerService, _spectacleService))
            {
                buyersForm.ShowDialog();
            }
        }
    }
}

