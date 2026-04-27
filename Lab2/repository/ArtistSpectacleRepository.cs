using System.Data;
using Lab2.domain;
using Microsoft.Data.Sqlite;

namespace Lab2.repository
{
    public class ArtistSpectacleRepository : IArtistSpectacleRepository
    {
        private readonly string _connectionString;

        public ArtistSpectacleRepository(string connectionString)
        {
            _connectionString = connectionString;
            InitializeTable();
        }

        private void InitializeTable()
        {
            const string sql = @"CREATE TABLE IF NOT EXISTS artist_spectacle (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    artist_id INTEGER NOT NULL,
                                    spectacle_id INTEGER NOT NULL,
                                    FOREIGN KEY(artist_id) REFERENCES artisti(ida) ON DELETE CASCADE,
                                    FOREIGN KEY(spectacle_id) REFERENCES spectacole(ids) ON DELETE CASCADE,
                                    UNIQUE(artist_id, spectacle_id)
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

        public ArtistSpectacle FindOne(long id)
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
            const string sql = "SELECT id, artist_id, spectacle_id FROM artist_spectacle";

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

            return relations;
        }

        public ArtistSpectacle Save(ArtistSpectacle entity)
        {
            if (ExistsRelation(entity.ArtistId, entity.SpectacleId))
            {
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

                    entity.Id = (long)command.ExecuteScalar();
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
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public ArtistSpectacle? Update(ArtistSpectacle entity)
        {
            if (!entity.Id.HasValue)
            {
                throw new ArgumentException("ArtistSpectacle sau ID nu poate fi null pentru update.");
            }

            const string sql = "UPDATE artist_spectacle SET artist_id = @artistId, spectacle_id = @spectacleId WHERE id = @id";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@artistId", entity.ArtistId);
                    command.Parameters.AddWithValue("@spectacleId", entity.SpectacleId);
                    command.Parameters.AddWithValue("@id", entity.Id.Value);

                    var rows = command.ExecuteNonQuery();
                    return rows == 0 ? null : entity;
                }
            }
        }

        public IEnumerable<ArtistSpectacle> FindByArtist(long artistId)
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

        public IEnumerable<ArtistSpectacle> FindBySpectacle(long spectacleId)
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
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        private static ArtistSpectacle ExtractArtistSpectacle(IDataRecord reader)
        {
            var id = reader.GetInt64(reader.GetOrdinal("id"));
            var artistId = reader.GetInt64(reader.GetOrdinal("artist_id"));
            var spectacleId = reader.GetInt64(reader.GetOrdinal("spectacle_id"));
            return new ArtistSpectacle(id, artistId, spectacleId);
        }
    }
}

