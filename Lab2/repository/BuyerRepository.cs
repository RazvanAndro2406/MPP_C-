using System.Data;
using Lab2.domain;
using Microsoft.Data.Sqlite;
using org.example.repository;

namespace Lab2.repository
{
    public class BuyerRepository : IBuyerRepository
    {
        private readonly string _connectionString;

        public BuyerRepository(string connectionString)
        {
            _connectionString = connectionString;
            InitializeTable();
        }

        private void InitializeTable()
        {
            const string sql = @"CREATE TABLE IF NOT EXISTS cumparatori (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    name TEXT NOT NULL
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

        public Buyer FindOne(long id)
        {
            const string sql = "SELECT id, name FROM cumparatori WHERE id = @id";

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
                            return ExtractBuyer(reader);
                        }
                    }
                }
            }

            return null;
        }

        public IEnumerable<Buyer> FindAll()
        {
            var buyers = new List<Buyer>();
            const string sql = "SELECT id, name FROM cumparatori";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        buyers.Add(ExtractBuyer(reader));
                    }
                }
            }

            return buyers;
        }

        public Buyer Save(Buyer entity)
        {
            const string sql = "INSERT INTO cumparatori (name) VALUES (@name); SELECT last_insert_rowid();";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", entity.Name);
                    entity.Id = (long)command.ExecuteScalar();
                    return entity;
                }
            }
        }

        public bool Delete(long id)
        {
            const string sql = "DELETE FROM cumparatori WHERE id = @id";

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

        public Buyer Update(Buyer entity)
        {
            const string sql = "UPDATE cumparatori SET name = @name WHERE id = @id";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", entity.Id);
                    command.Parameters.AddWithValue("@name", entity.Name);
                    var rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected == 0 ? entity : null;
                }
            }
        }

        private static Buyer ExtractBuyer(IDataRecord reader)
        {
            var id = reader.GetInt64(reader.GetOrdinal("id"));
            var name = reader.GetString(reader.GetOrdinal("name"));
            return new Buyer(id, name);
        }
    }
}

