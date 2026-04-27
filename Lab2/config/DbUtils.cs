using System.Configuration; 
using System.Data;
using Microsoft.Data.Sqlite;
using log4net;

namespace org.example.repository {
    public static class DbUtils { 
        private static readonly ILog log = LogManager.GetLogger(typeof(DbUtils));
        private static IDbConnection _instance = null;

        public static IDbConnection GetConnection() {
            log.Info("Se solicita o conexiune la DB...");
            try {
                if (_instance == null || _instance.State == ConnectionState.Closed) {
                    log.Info("Creare conexiune noua (citire din App.config)...");
                    
                    string connectionString = ConfigurationManager.ConnectionStrings["spectacoleDB"].ConnectionString;
                    
                    _instance = new SqliteConnection(connectionString);
                    _instance.Open();
                    log.Info("Conexiune deschisă cu succes.");
                }
            } catch (Exception e) {
                log.Error("Eroare gravă la stabilirea conexiunii: ", e);
                throw; 
            }
            return _instance;
        }
    }
}