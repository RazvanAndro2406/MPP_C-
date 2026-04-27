using log4net;
using System.Data;
using Lab2.domain;
using Microsoft.Data.Sqlite;
using Org.Example.Domain;
using org.example.repository;

namespace Lab2.repository
{
    public class ArtistRepository : IArtistRepository
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ArtistRepository));
        private readonly string _connectionString;

        public ArtistRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Artist FindOne(long id)
        {
            log.InfoFormat("Cautare artist cu ID: {0}", id);
            const string sql = "SELECT * FROM artisti WHERE ida = @id";
            
            IDbConnection connection = DbUtils.GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                var param = command.CreateParameter();
                param.ParameterName = "@id";
                param.Value = id;
                command.Parameters.Add(param);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return ExtractArtist(reader);
                    }
                }
            }
            return null;
        }

        public IEnumerable<Artist> FindAll()
        {
            log.Info("Se extrag toți artiștii...");
            var artisti = new List<Artist>();
            const string sql = "SELECT * FROM Artisti";

            IDbConnection connection = DbUtils.GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        artisti.Add(ExtractArtist(reader));
                    }
                }
            }

            log.InfoFormat("S-au incarcat {0} artisti.", artisti.Count);
            return artisti;
        }

        public Artist Save(Artist entity)
        {
            log.InfoFormat("Salvare artist: {0}", entity.Name);
            const string sql = "INSERT INTO artisti (name) VALUES (@name); SELECT last_insert_rowid();";

            IDbConnection connection = DbUtils.GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                var param = command.CreateParameter();
                param.ParameterName = "@name";
                param.Value = entity.Name;
                command.Parameters.Add(param);

                entity.Id = (long)command.ExecuteScalar();
                log.InfoFormat("Artist salvat cu succes, ID generat: {0}", entity.Id);
                return entity;
            }
        }

        private Artist ExtractArtist(IDataReader reader)
        {
            long id = reader.GetInt64(reader.GetOrdinal("ida"));
            string name = reader.GetString(reader.GetOrdinal("name"));
            return new Artist(id, name);
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
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public Artist Update(Artist entity)
        {
            if (entity == null || entity.Id == null)
                throw new ArgumentException("Entity or ID cannot be null for update!");

            const string sql = @"UPDATE artisti SET name = @name where ida = @id";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", entity.Name);
                    command.Parameters.AddWithValue("@id", entity.Id);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected == 0 ? entity : null;
                }
            }
        }
    }
}