using System.Configuration;
using System.Data;
using log4net;
using Microsoft.Data.Sqlite;

namespace org.example.repository
{
    public static class DbUtils
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DbUtils));
        private static IDbConnection? _instance;

        public static IDbConnection GetConnection()
        {
            log.Info("Se solicita o conexiune la DB...");
            try
            {
                if (_instance == null || _instance.State == ConnectionState.Closed)
                {
                    log.Info("Creare conexiune noua (citire din App.config)...");

                    var connectionString = ConfigurationManager.ConnectionStrings["spectacoleDB"]?.ConnectionString;
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException("spectacoleDB connection string not found in App.config");
                    }

                    _instance = new SqliteConnection(connectionString);
                    _instance.Open();
                    log.Info("Conexiune deschisă cu succes.");
                }
            }
            catch (Exception e)
            {
                log.Error("Eroare gravă la stabilirea conexiunii: ", e);
                throw;
            }

            return _instance;
        }

        public static void Initialize(string connectionString)
        {
            log.Info($"Database initialized: {connectionString}");
        }

        public static void InitializeSchema(string connectionString)
        {
            // Schema already exists in Lab1 database
            log.Info("Database schema validation skipped (using existing Lab1 database)");
        }

        public static void SeedData(string connectionString)
        {
            // No seed needed for Lab1 database
            log.Info("Using existing Lab1 data (no seed needed)");
        }
    }
}

