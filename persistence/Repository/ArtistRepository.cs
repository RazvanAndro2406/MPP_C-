using System.Data;
using log4net;
using Microsoft.Data.Sqlite;
using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository;

public sealed class ArtistRepository : IArtistRepository
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(ArtistRepository));
    private readonly string _connectionString;

    public ArtistRepository(string connectionString)
    {
        _connectionString = connectionString;
        Logger.Info("ArtistRepository initialized");
    }

    public Artist? FindOne(long id)
    {
        const string sql = "SELECT ida, name FROM artisti WHERE ida = @id";

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
                        return ExtractArtist(reader);
                    }
                }
            }
        }

        return null;
    }

    public IEnumerable<Artist> FindAll()
    {
        var artists = new List<Artist>();
        const string sql = "SELECT ida, name FROM artisti ORDER BY name";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    artists.Add(ExtractArtist(reader));
                }
            }
        }

        Logger.Info($"Found {artists.Count} artists");
        return artists;
    }

    public Artist Save(Artist entity)
    {
        const string sql = "INSERT INTO artisti (name) VALUES (@name); SELECT last_insert_rowid();";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@name", entity.Name);
                entity.Id = (long)command.ExecuteScalar()!;
                Logger.Info($"Artist saved: {entity.Name} with id: {entity.Id}");
                return entity;
            }
        }
    }

    public bool Delete(long id)
    {
        const string sql = "DELETE FROM artisti WHERE ida = @id";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                bool deleted = command.ExecuteNonQuery() > 0;
                if (deleted)
                {
                    Logger.Info($"Artist deleted: id={id}");
                }
                return deleted;
            }
        }
    }

    public Artist? Update(Artist entity)
    {
        const string sql = "UPDATE artisti SET name = @name WHERE ida = @id";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@name", entity.Name);
                command.Parameters.AddWithValue("@id", entity.Id);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Logger.Info($"Artist updated: {entity.Name}");
                    return null;
                }

                Logger.Warn($"Artist update failed for id: {entity.Id}");
                return entity;
            }
        }
    }

    private static Artist ExtractArtist(IDataRecord reader)
    {
        long id = reader.GetInt64(reader.GetOrdinal("ida"));
        string name = reader.GetString(reader.GetOrdinal("name"));
        return new Artist(id, name);
    }
}

