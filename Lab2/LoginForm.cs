using System;
using System.Drawing;
using System.Windows.Forms;
using Lab2.domain;
using Lab2.repository;

namespace Lab2
{
    public class LoginForm : Form
    {
        private readonly TextBox _usernameTextBox;
        private readonly TextBox _passwordTextBox;
        private readonly Label _statusLabel;
        private readonly UserRepository _userRepository;

        public User? AuthenticatedUser { get; private set; }

        public LoginForm(string connectionString)
        {
            _userRepository = new UserRepository(connectionString);

            Text = "Login";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(380, 220);

            var usernameLabel = new Label
            {
                Text = "Username",
                Location = new Point(25, 30),
                AutoSize = true
            };

            _usernameTextBox = new TextBox
            {
                Location = new Point(120, 25),
                Width = 220
            };

            var passwordLabel = new Label
            {
                Text = "Parola",
                Location = new Point(25, 80),
                AutoSize = true
            };

            _passwordTextBox = new TextBox
            {
                Location = new Point(120, 75),
                Width = 220,
                UseSystemPasswordChar = true
            };

            var loginButton = new Button
            {
                Text = "Login",
                Location = new Point(120, 125),
                Width = 100
            };
            loginButton.Click += LoginButton_Click;

            _statusLabel = new Label
            {
                Location = new Point(25, 170),
                Width = 335,
                ForeColor = Color.DarkRed,
                AutoSize = false
            };

            AcceptButton = loginButton;

            Controls.Add(usernameLabel);
            Controls.Add(_usernameTextBox);
            Controls.Add(passwordLabel);
            Controls.Add(_passwordTextBox);
            Controls.Add(loginButton);
            Controls.Add(_statusLabel);
        }

        private void LoginButton_Click(object? sender, EventArgs e)
        {
            var username = _usernameTextBox.Text.Trim();
            var password = _passwordTextBox.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = "Completeaza username si parola.";
                return;
            }

            try
            {
                var user = _userRepository.Authenticate(username, password);
                if (user == null)
                {
                    _statusLabel.ForeColor = Color.DarkRed;
                    _statusLabel.Text = "Date invalide.";
                    return;
                }

                AuthenticatedUser = user;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.DarkRed;
                _statusLabel.Text = $"Eroare la autentificare: {ex.Message}";
            }
        }
    }
}

