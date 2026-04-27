using System;
using System.Configuration;
using System.Windows.Forms;

namespace Lab2
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

            var connString = ConfigurationManager.ConnectionStrings["spectacoleDB"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connString))
            {
                MessageBox.Show("Lipseste conexiunea 'spectacoleDB' din app.config.", "Configurare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var loginForm = new LoginForm(connString))
            {
                if (loginForm.ShowDialog() == DialogResult.OK && loginForm.AuthenticatedUser != null)
                {
                    Application.Run(new MainForm(loginForm.AuthenticatedUser));
                }
            }
        }
    }
}