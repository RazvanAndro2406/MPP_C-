using Microsoft.Data.Sqlite;
using Lab2.domain;
using Lab2.repository;

namespace org.example.repository
{
    public class SpectacleRepository:ISpectacleRepository
    {
        private readonly string _connectionString;
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public SpectacleRepository(string connectionString)
        {
           
            _connectionString = connectionString;

            InitializeDatabase();
            InitializeTicketSalesTable();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();
            }
        }

        private void InitializeTicketSalesTable()
        {
            const string sql = @"CREATE TABLE IF NOT EXISTS ticket_sales (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    spectacle_id INTEGER NOT NULL,
                                    buyer_id INTEGER NOT NULL,
                                    buyer_name TEXT NOT NULL,
                                    seats INTEGER NOT NULL,
                                    sold_at TEXT NOT NULL,
                                    FOREIGN KEY(spectacle_id) REFERENCES spectacole(ids) ON DELETE CASCADE,
                                    FOREIGN KEY(buyer_id) REFERENCES cumparatori(id) ON DELETE CASCADE
                                );";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public Spectacle FindOne(long id)
        {
            const string sql = "SELECT * FROM spectacole WHERE ids = @id";
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
                            return ExtractSpectacle(reader);
                        }
                    }
                }
            }
            return null; 
        }

        
        public IEnumerable<Spectacle> FindAll()
        {
            var spectacles = new List<Spectacle>();
            const string sql = "SELECT * FROM spectacole";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        spectacles.Add(ExtractSpectacle(reader));
                    }
                }
            }
            return spectacles;
        }

        public Spectacle Save(Spectacle entity)
        {
            const string sql = @"INSERT INTO spectacole (name, start_date, duration, capacity, location) 
                                VALUES (@name, @start_date, @duration, @capacity, @location);
                                SELECT last_insert_rowid();";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", entity.Name);
                    command.Parameters.AddWithValue("@start_date", entity.StartDate.ToString(DateTimeFormat));
                    command.Parameters.AddWithValue("@duration", entity.Duration);
                    command.Parameters.AddWithValue("@capacity", entity.Capacity);
                    command.Parameters.AddWithValue("@location", entity.Location);

                    
                    var id = (long)command.ExecuteScalar();
                    entity.Id = id;
                    
                    return null; 
                }
            }
        }

        public bool Delete(long id)
        {
            const string sql = "DELETE FROM spectacole WHERE ids = @id";
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

        public Spectacle Update(Spectacle entity)
        {
            if (entity == null || entity.Id == null)
                throw new ArgumentException("Entity or ID cannot be null for update!");

            const string sql = @"UPDATE spectacole SET name = @name, start_date = @start_date, 
                                duration = @duration, capacity = @capacity, location = @location 
                                WHERE ids = @id";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", entity.Name);
                    command.Parameters.AddWithValue("@start_date", entity.StartDate.ToString(DateTimeFormat));
                    command.Parameters.AddWithValue("@duration", entity.Duration);
                    command.Parameters.AddWithValue("@capacity", entity.Capacity);
                    command.Parameters.AddWithValue("@location", entity.Location);
                    command.Parameters.AddWithValue("@id", entity.Id);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected == 0 ? entity : null;
                }
            }
        }

        public bool DeleteByName(string name)
        {
            const string sql = "DELETE FROM spectacole WHERE name = @name";
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public int GetAvailableSeats(long spectacleId)
        {
            const string sql = "SELECT capacity FROM spectacole WHERE ids = @id";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", spectacleId);
                    var result = command.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                    {
                        return 0;
                    }

                    return Math.Max(0, Convert.ToInt32(result));
                }
            }
        }

        public bool SellTickets(long spectacleId, long buyerId, string buyerName, int numberOfSeats)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var available = GetAvailableSeatsInternal(connection, transaction, spectacleId);
                    if (available < numberOfSeats)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    const string updateSpectacleSql = @"UPDATE spectacole
                                                        SET capacity = capacity - @seats
                                                        WHERE ids = @spectacleId AND capacity >= @seats";

                    using (var updateCommand = new SqliteCommand(updateSpectacleSql, connection, transaction))
                    {
                        updateCommand.Parameters.AddWithValue("@spectacleId", spectacleId);
                        updateCommand.Parameters.AddWithValue("@seats", numberOfSeats);
                        var rowsUpdated = updateCommand.ExecuteNonQuery();
                        if (rowsUpdated == 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                    const string insertSql = @"INSERT INTO ticket_sales (spectacle_id, buyer_id, buyer_name, seats, sold_at)
                                               VALUES (@spectacleId, @buyerId, @buyerName, @seats, @soldAt)";

                    using (var command = new SqliteCommand(insertSql, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@spectacleId", spectacleId);
                        command.Parameters.AddWithValue("@buyerId", buyerId);
                        command.Parameters.AddWithValue("@buyerName", buyerName);
                        command.Parameters.AddWithValue("@seats", numberOfSeats);
                        command.Parameters.AddWithValue("@soldAt", DateTime.UtcNow.ToString(DateTimeFormat));
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
            }
        }

        public IEnumerable<TicketSale> GetTicketSalesByBuyer(long buyerId)
        {
            var sales = new List<TicketSale>();
            const string sql = @"SELECT ts.id, ts.spectacle_id, ts.buyer_id, ts.buyer_name, ts.seats, ts.sold_at, s.name AS spectacle_name
                                 FROM ticket_sales ts
                                 INNER JOIN spectacole s ON s.ids = ts.spectacle_id
                                 WHERE ts.buyer_id = @buyerId
                                 ORDER BY ts.sold_at DESC";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@buyerId", buyerId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sales.Add(ExtractTicketSale(reader));
                        }
                    }
                }
            }

            return sales;
        }

        public bool IncreaseTicketSeats(long ticketSaleId, int extraSeats)
        {
            if (extraSeats <= 0)
            {
                return false;
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    const string findSql = "SELECT spectacle_id FROM ticket_sales WHERE id = @ticketSaleId";
                    long spectacleId;

                    using (var findCommand = new SqliteCommand(findSql, connection, transaction))
                    {
                        findCommand.Parameters.AddWithValue("@ticketSaleId", ticketSaleId);
                        var result = findCommand.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        spectacleId = Convert.ToInt64(result);
                    }

                    const string updateSpectacleSql = @"UPDATE spectacole
                                                        SET capacity = capacity - @extraSeats
                                                        WHERE ids = @spectacleId AND capacity >= @extraSeats";

                    using (var updateSpectacleCommand = new SqliteCommand(updateSpectacleSql, connection, transaction))
                    {
                        updateSpectacleCommand.Parameters.AddWithValue("@spectacleId", spectacleId);
                        updateSpectacleCommand.Parameters.AddWithValue("@extraSeats", extraSeats);
                        if (updateSpectacleCommand.ExecuteNonQuery() == 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                    const string updateSaleSql = "UPDATE ticket_sales SET seats = seats + @extraSeats WHERE id = @ticketSaleId";
                    using (var updateSaleCommand = new SqliteCommand(updateSaleSql, connection, transaction))
                    {
                        updateSaleCommand.Parameters.AddWithValue("@extraSeats", extraSeats);
                        updateSaleCommand.Parameters.AddWithValue("@ticketSaleId", ticketSaleId);
                        if (updateSaleCommand.ExecuteNonQuery() == 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                    transaction.Commit();
                    return true;
                }
            }
        }

        private static int GetAvailableSeatsInternal(SqliteConnection connection, SqliteTransaction transaction, long spectacleId)
        {
            const string sql = "SELECT capacity FROM spectacole WHERE ids = @id";

            using (var command = new SqliteCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@id", spectacleId);
                var result = command.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    return 0;
                }

                return Math.Max(0, Convert.ToInt32(result));
            }
        }
        
        private Spectacle ExtractSpectacle(SqliteDataReader reader)
        {
            long id = reader.GetInt64(reader.GetOrdinal("ids"));
            string name = reader.GetString(reader.GetOrdinal("name"));
            string dateStr = reader.GetString(reader.GetOrdinal("start_date"));
            int duration = reader.GetInt32(reader.GetOrdinal("duration"));
            int capacity = reader.GetInt32(reader.GetOrdinal("capacity"));
            string location = reader.GetString(reader.GetOrdinal("location"));
            
            DateTime date = DateTime.ParseExact(dateStr, DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);

            return new Spectacle(id, name, date, duration, capacity, location);
        }

        private TicketSale ExtractTicketSale(SqliteDataReader reader)
        {
            var id = reader.GetInt64(reader.GetOrdinal("id"));
            var spectacleId = reader.GetInt64(reader.GetOrdinal("spectacle_id"));
            var buyerId = reader.GetInt64(reader.GetOrdinal("buyer_id"));
            var buyerName = reader.GetString(reader.GetOrdinal("buyer_name"));
            var seats = reader.GetInt32(reader.GetOrdinal("seats"));
            var soldAtRaw = reader.GetString(reader.GetOrdinal("sold_at"));
            var spectacleName = reader.GetString(reader.GetOrdinal("spectacle_name"));

            var soldAt = DateTime.ParseExact(soldAtRaw, DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
            return new TicketSale(id, spectacleId, buyerId, buyerName, seats, soldAt, spectacleName);
        }
    }
    
    
}