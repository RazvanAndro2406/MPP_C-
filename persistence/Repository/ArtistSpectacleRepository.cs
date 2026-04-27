using System.Data;
using log4net;
using Microsoft.Data.Sqlite;
using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository;

public sealed class ArtistSpectacleRepository : IArtistSpectacleRepository
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(ArtistSpectacleRepository));
    private readonly string _connectionString;

    public ArtistSpectacleRepository(string connectionString)
    {
        _connectionString = connectionString;
        Logger.Info("ArtistSpectacleRepository initialized");
    }

    public ArtistSpectacle? FindOne(long id)
    {
        const string sql = "SELECT id, artist_id, spectacle_id FROM artist_spectacle WHERE id = @id";

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
                        return ExtractArtistSpectacle(reader);
                    }
                }
            }
        }

        return null;
    }

    public IEnumerable<ArtistSpectacle> FindAll()
    {
        var relations = new List<ArtistSpectacle>();
        const string sql = "SELECT id, artist_id, spectacle_id FROM artist_spectacle ORDER BY artist_id, spectacle_id";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    relations.Add(ExtractArtistSpectacle(reader));
                }
            }
        }

        Logger.Info($"Found {relations.Count} artist-spectacle relations");
        return relations;
    }

    public ArtistSpectacle Save(ArtistSpectacle entity)
    {
        if (ExistsRelation(entity.ArtistId, entity.SpectacleId))
        {
            Logger.Warn($"Relation already exists: artist {entity.ArtistId}, spectacle {entity.SpectacleId}");
            return entity;
        }

        const string sql = "INSERT INTO artist_spectacle (artist_id, spectacle_id) VALUES (@artistId, @spectacleId); SELECT last_insert_rowid();";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@artistId", entity.ArtistId);
                command.Parameters.AddWithValue("@spectacleId", entity.SpectacleId);

                entity.Id = (long)command.ExecuteScalar()!;
                Logger.Info($"Artist-spectacle relation saved: artist {entity.ArtistId}, spectacle {entity.SpectacleId}");
                return entity;
            }
        }
    }

    public bool Delete(long id)
    {
        const string sql = "DELETE FROM artist_spectacle WHERE id = @id";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                bool deleted = command.ExecuteNonQuery() > 0;
                if (deleted)
                {
                    Logger.Info($"Artist-spectacle relation deleted: id={id}");
                }
                return deleted;
            }
        }
    }

    public ArtistSpectacle? Update(ArtistSpectacle entity)
    {
        const string sql = "UPDATE artist_spectacle SET artist_id = @artistId, spectacle_id = @spectacleId WHERE id = @id";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@artistId", entity.ArtistId);
                command.Parameters.AddWithValue("@spectacleId", entity.SpectacleId);
                command.Parameters.AddWithValue("@id", entity.Id);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Logger.Info($"Artist-spectacle relation updated: id={entity.Id}");
                    return null;
                }

                Logger.Warn($"Artist-spectacle relation update failed for id: {entity.Id}");
                return entity;
            }
        }
    }

    public IEnumerable<ArtistSpectacle> FindByArtistId(long artistId)
    {
        var relations = new List<ArtistSpectacle>();
        const string sql = "SELECT id, artist_id, spectacle_id FROM artist_spectacle WHERE artist_id = @artistId";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@artistId", artistId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        relations.Add(ExtractArtistSpectacle(reader));
                    }
                }
            }
        }

        return relations;
    }

    public IEnumerable<ArtistSpectacle> FindBySpectacleId(long spectacleId)
    {
        var relations = new List<ArtistSpectacle>();
        const string sql = "SELECT id, artist_id, spectacle_id FROM artist_spectacle WHERE spectacle_id = @spectacleId";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@spectacleId", spectacleId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        relations.Add(ExtractArtistSpectacle(reader));
                    }
                }
            }
        }

        return relations;
    }

    public bool ExistsRelation(long artistId, long spectacleId)
    {
        const string sql = "SELECT COUNT(*) FROM artist_spectacle WHERE artist_id = @artistId AND spectacle_id = @spectacleId";

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@artistId", artistId);
                command.Parameters.AddWithValue("@spectacleId", spectacleId);
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result) > 0;
            }
        }
    }

    private static ArtistSpectacle ExtractArtistSpectacle(IDataRecord reader)
    {
        long id = reader.GetInt64(reader.GetOrdinal("id"));
        long artistId = reader.GetInt64(reader.GetOrdinal("artist_id"));
        long spectacleId = reader.GetInt64(reader.GetOrdinal("spectacle_id"));
        return new ArtistSpectacle(id, artistId, spectacleId);
    }
}

