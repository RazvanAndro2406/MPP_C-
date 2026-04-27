using System.Data;
using log4net;
using Microsoft.Data.Sqlite;
using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository;

public sealed class BuyerRepository : IBuyerRepository
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(BuyerRepository));
    private readonly string _connectionString;

    public BuyerRepository(string connectionString)
    {
        _connectionString = connectionString;
        Logger.Info("BuyerRepository initialized");
    }

    public Buyer? FindOne(long id)
    {
        // Added email to SELECT
        const string sql = "SELECT id, name, email FROM cumparatori WHERE id = @id";

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
        // Added email to SELECT
        const string sql = "SELECT id, name, email FROM cumparatori ORDER BY name";

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

        Logger.Info($"Found {buyers.Count} buyers");
        return buyers;
    }

    public Buyer Save(Buyer entity)
    {
        // Added email to INSERT
        const string sql = "INSERT INTO cumparatori (name, email) VALUES (@name, @email); SELECT last_insert_rowid();";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@name", entity.Name);
                command.Parameters.AddWithValue("@email", entity.Email); // Added parameter
                entity.Id = (long)command.ExecuteScalar()!;
                Logger.Info($"Buyer saved: {entity.Name} ({entity.Email}) with id: {entity.Id}");
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
                bool deleted = command.ExecuteNonQuery() > 0;
                if (deleted)
                {
                    Logger.Info($"Buyer deleted: id={id}");
                }
                return deleted;
            }
        }
    }

    public Buyer? Update(Buyer entity)
    {
        // Added email to UPDATE
        const string sql = "UPDATE cumparatori SET name = @name, email = @email WHERE id = @id";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@name", entity.Name);
                command.Parameters.AddWithValue("@email", entity.Email); // Added parameter
                command.Parameters.AddWithValue("@id", entity.Id);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Logger.Info($"Buyer updated: {entity.Name}");
                    return null;
                }

                Logger.Warn($"Buyer update failed for id: {entity.Id}");
                return entity;
            }
        }
    }

    public Buyer? FindByName(string name)
    {
        const string sql = "SELECT id, name, email FROM cumparatori WHERE name = @name LIMIT 1";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@name", name);
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

    // Recommended: Add FindByEmail since you used it in your Java logic
    public Buyer? FindByEmail(string email)
    {
        const string sql = "SELECT id, name, email FROM cumparatori WHERE email = @email LIMIT 1";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@email", email);
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

    private static Buyer ExtractBuyer(IDataRecord reader)
    {
        long id = reader.GetInt64(reader.GetOrdinal("id"));
        string name = reader.GetString(reader.GetOrdinal("name"));
        string email = reader.GetString(reader.GetOrdinal("email")); // Read email
        return new Buyer(id, name, email); // Use updated constructor
    }
}