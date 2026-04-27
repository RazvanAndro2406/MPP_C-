using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using Microsoft.Data.Sqlite;
using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository
{
    public class TicketRepository : ITicketRepository
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TicketRepository));
        private readonly string _connectionString;

        public TicketRepository(string connectionString)
        {
            Logger.Info("Initializing TicketRepository with connection string");
            _connectionString = connectionString;
        }

        public Ticket? FindOne(long id)
        {
            const string sql = "SELECT * FROM tichete WHERE idt = @id";
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ExtractTicket(reader);
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<Ticket> FindAll()
        {
            IList<Ticket> tickets = new List<Ticket>();
            const string sql = "SELECT * FROM tichete";
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tickets.Add(ExtractTicket(reader));
                    }
                }
            }
            return tickets;
        }

        public Ticket? Save(Ticket entity)
        {
            const string sql = "INSERT INTO tichete (price, ids) VALUES (@price, @ids); SELECT last_insert_rowid();";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@price", entity.Price);
                    command.Parameters.AddWithValue("@ids", entity.SpectacleId);

                    var result = command.ExecuteScalar();
                    if (result == null) return entity; // Failure logic matching your Java code

                    entity.Id = Convert.ToInt64(result);
                    return null; // Success logic matching Optional.empty()
                }
            }
        }

        public bool Delete(long id)
        {
            const string sql = "DELETE FROM tichete WHERE idt = @id";
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqliteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqliteException e)
            {
                Logger.Error("Error deleting ticket", e);
                throw new Exception("Error deleting ticket", e);
            }
        }

        public Ticket? Update(Ticket entity)
        {
            if (entity == null)
            {
                throw new ArgumentException("Ticket cannot be null for update!");
            }

            const string sql = "UPDATE tichete SET price = @price, ids = @ids WHERE idt = @id";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@price", entity.Price);
                    command.Parameters.AddWithValue("@ids", entity.SpectacleId);
                    command.Parameters.AddWithValue("@id", entity.Id);

                    int rowsAffected = command.ExecuteNonQuery();
                    // Returns entity if 0 rows affected (fail), null if successful (success)
                    return rowsAffected == 0 ? entity : null;
                }
            }
        }

        private Ticket ExtractTicket(IDataRecord reader)
        {
            long id = reader.GetInt64(reader.GetOrdinal("idt"));
            double price = reader.GetDouble(reader.GetOrdinal("price"));
            long spectacleId = reader.GetInt64(reader.GetOrdinal("ids"));
            return new Ticket(id, price, spectacleId);
        }
    }
}