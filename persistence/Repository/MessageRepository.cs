using System;
using log4net;
using Microsoft.Data.Sqlite;
using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly string _connectionString;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageRepository));

        public MessageRepository(string connectionString)
        {
            Logger.Info("Initializing MessageRepository");
            _connectionString = connectionString;
            EnsureSchema();
        }

        private void EnsureSchema()
        {
            const string sql = @"CREATE TABLE IF NOT EXISTS messages (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                sender TEXT NOT NULL,
                receiver TEXT NOT NULL,
                body TEXT NOT NULL,
                sent_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            )";

            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqliteCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqliteException e)
            {
                throw new Exception("Error ensuring messages schema", e);
            }
        }

        public void Save(Message message)
        {
            const string sql = "INSERT INTO messages(sender, receiver, body) VALUES (@sender, @receiver, @body)";
            
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqliteCommand(sql, connection))
                    {
                        // Matches Java: message.getSender().getId()
                        // Note: If your User.Id is a long, we use .ToString() to match the TEXT column in your schema
                        command.Parameters.AddWithValue("@sender", message.Sender.Id.ToString());
                        command.Parameters.AddWithValue("@receiver", message.Receiver.Id.ToString());
                        command.Parameters.AddWithValue("@body", message.Text);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqliteException e)
            {
                throw new Exception("Error saving message", e);
            }
        }
    }
}