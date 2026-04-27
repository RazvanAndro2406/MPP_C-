using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;
using Microsoft.Data.Sqlite;
using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository
{
    public class TicketSaleRepository : ITicketSaleRepository
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TicketSaleRepository));
        private const string DbDateFormat = "yyyy-MM-dd HH:mm:ss";
        private readonly string _connectionString;

        public TicketSaleRepository(string connectionString)
        {
            Logger.Info("Initializing TicketSaleRepository");
            _connectionString = connectionString;
            InitTable();
        }

        private void InitTable()
        {
            const string sql = @"CREATE TABLE IF NOT EXISTS ticket_sales (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                spectacle_id INTEGER NOT NULL,
                buyer_id INTEGER NOT NULL,
                buyer_name TEXT NOT NULL,
                seats INTEGER NOT NULL,
                sold_at TEXT NOT NULL,
                FOREIGN KEY(spectacle_id) REFERENCES spectacole(ids),
                FOREIGN KEY(buyer_id) REFERENCES cumparatori(id)
            )";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public TicketSale? FindOne(long id)
        {
            const string sql = @"SELECT ts.id, ts.spectacle_id, ts.buyer_id, ts.buyer_name, ts.seats, ts.sold_at, s.name AS spectacle_name 
                                FROM ticket_sales ts LEFT JOIN spectacole s ON s.ids = ts.spectacle_id WHERE ts.id = @id";
            
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return ExtractSale(reader);
                    }
                }
            }
            return null;
        }

        public IEnumerable<TicketSale> FindAll()
        {
            var sales = new List<TicketSale>();
            const string sql = @"SELECT ts.id, ts.spectacle_id, ts.buyer_id, ts.buyer_name, ts.seats, ts.sold_at, s.name AS spectacle_name 
                                FROM ticket_sales ts LEFT JOIN spectacole s ON s.ids = ts.spectacle_id ORDER BY ts.id";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read()) sales.Add(ExtractSale(reader));
                }
            }
            return sales;
        }

        public TicketSale? Save(TicketSale entity)
        {
            const string sql = @"INSERT INTO ticket_sales (spectacle_id, buyer_id, buyer_name, seats, sold_at) 
                                VALUES (@sid, @bid, @bn, @s, @dat); SELECT last_insert_rowid();";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@sid", entity.SpectacleId);
                    command.Parameters.AddWithValue("@bid", entity.BuyerId);
                    command.Parameters.AddWithValue("@bn", entity.BuyerName);
                    command.Parameters.AddWithValue("@s", entity.Seats);
                    command.Parameters.AddWithValue("@dat", entity.SoldAt.ToString(DbDateFormat));

                    var result = command.ExecuteScalar();
                    if (result == null) return entity;

                    entity.Id = Convert.ToInt64(result);
                    return null; // Success = null (1:1 with Optional.empty)
                }
            }
        }

        public bool Delete(long id)
        {
            const string sql = "DELETE FROM ticket_sales WHERE id = @id";
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

        public TicketSale? Update(TicketSale entity)
        {
            const string sql = @"UPDATE ticket_sales SET spectacle_id = @sid, buyer_id = @bid, 
                                buyer_name = @bn, seats = @s, sold_at = @dat WHERE id = @id";
            
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@sid", entity.SpectacleId);
                    command.Parameters.AddWithValue("@bid", entity.BuyerId);
                    command.Parameters.AddWithValue("@bn", entity.BuyerName);
                    command.Parameters.AddWithValue("@s", entity.Seats);
                    command.Parameters.AddWithValue("@dat", entity.SoldAt.ToString(DbDateFormat));
                    command.Parameters.AddWithValue("@id", entity.Id);

                    return command.ExecuteNonQuery() == 0 ? entity : null;
                }
            }
        }

        public IEnumerable<TicketSale> FindByBuyerId(long buyerId)
        {
            var sales = new List<TicketSale>();
            const string sql = @"SELECT ts.id, ts.spectacle_id, ts.buyer_id, ts.buyer_name, ts.seats, ts.sold_at, s.name AS spectacle_name 
                                FROM ticket_sales ts LEFT JOIN spectacole s ON s.ids = ts.spectacle_id WHERE ts.buyer_id = @bid ORDER BY ts.id";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@bid", buyerId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) sales.Add(ExtractSale(reader));
                    }
                }
            }
            return sales;
        }

        public int GetSoldSeatsForSpectacle(long spectacleId)
        {
            const string sql = "SELECT COALESCE(SUM(seats), 0) FROM ticket_sales WHERE spectacle_id = @sid";
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@sid", spectacleId);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        private TicketSale ExtractSale(IDataRecord rs)
        {
            // C# DateTime.ParseExact is the equivalent of Java's DateTimeFormatter
            DateTime soldAt = DateTime.ParseExact(rs.GetString(rs.GetOrdinal("sold_at")), DbDateFormat, null);
            
            return new TicketSale(
                rs.GetInt64(rs.GetOrdinal("id")),
                rs.GetInt64(rs.GetOrdinal("spectacle_id")),
                rs.GetInt64(rs.GetOrdinal("buyer_id")),
                rs.GetString(rs.GetOrdinal("buyer_name")),
                rs.GetInt32(rs.GetOrdinal("seats")),
                soldAt,
                rs.IsDBNull(rs.GetOrdinal("spectacle_name")) ? "" : rs.GetString(rs.GetOrdinal("spectacle_name"))
            );
        }
    }
}