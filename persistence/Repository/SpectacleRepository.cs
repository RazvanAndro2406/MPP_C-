using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using Microsoft.Data.Sqlite;
using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository
{
    public class SpectacleRepository : ISpectacleRepository
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SpectacleRepository));
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";
        private readonly string _connectionString;

        public SpectacleRepository(string connectionString)
        {
            Logger.Info("Initializing SpectacleRepository with properties");
            _connectionString = connectionString;
        }

        public Spectacle? FindOne(long id)
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
                            Logger.Info("Spectacol gasit cu succes in baza de date.");
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

        public Spectacle? Save(Spectacle entity)
        {
            const string sql = @"INSERT INTO spectacole (name, start_date, duration, capacity, location) 
                                VALUES (@name, @date, @dur, @cap, @loc); SELECT last_insert_rowid();";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", entity.Name);
                    command.Parameters.AddWithValue("@date", entity.Start_date.ToString(DateFormat));
                    command.Parameters.AddWithValue("@dur", entity.Duration);
                    command.Parameters.AddWithValue("@cap", entity.Capacity);
                    command.Parameters.AddWithValue("@loc", entity.Location);

                    var result = command.ExecuteScalar();
                    if (result == null)
                    {
                        Logger.Info("Spectacol nu a fost salvat.");
                        return entity;
                    }

                    entity.Id = Convert.ToInt64(result);
                    return null;
                }
            }
        }

        public bool Delete(long id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                // Matches Java: connection.setAutoCommit(false)
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Delete Relations
                        const string deleteRelationsSql = "DELETE FROM artist_spectacle WHERE spectacle_id = @id";
                        using (var cmdRel = new SqliteCommand(deleteRelationsSql, connection, transaction))
                        {
                            cmdRel.Parameters.AddWithValue("@id", id);
                            cmdRel.ExecuteNonQuery();
                        }

                        // 2. Delete Spectacle
                        const string deleteSpectacleSql = "DELETE FROM spectacole WHERE ids = @id";
                        using (var cmdSpec = new SqliteCommand(deleteSpectacleSql, connection, transaction))
                        {
                            cmdSpec.Parameters.AddWithValue("@id", id);
                            int result = cmdSpec.ExecuteNonQuery();
                            
                            // Matches Java: connection.commit()
                            transaction.Commit();
                            return result > 0;
                        }
                    }
                    catch (SqliteException e)
                    {
                        // Matches Java: connection.rollback()
                        transaction.Rollback();
                        Logger.Error("Error deleting spectacle and relations", e);
                        throw new Exception("Error deleting spectacle", e);
                    }
                }
            }
        }

        public Spectacle? Update(Spectacle entity)
        {
            if (entity == null)
                throw new ArgumentException("Entitatea sau ID-ul nu pot fi null pentru update!");

            const string sql = @"UPDATE spectacole SET name = @name, start_date = @date, 
                                duration = @dur, capacity = @cap, location = @loc WHERE ids = @id";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", entity.Name);
                    command.Parameters.AddWithValue("@date", entity.Start_date.ToString(DateFormat));
                    command.Parameters.AddWithValue("@dur", entity.Duration);
                    command.Parameters.AddWithValue("@cap", entity.Capacity);
                    command.Parameters.AddWithValue("@loc", entity.Location);
                    command.Parameters.AddWithValue("@id", entity.Id);

                    int response = command.ExecuteNonQuery();
                    return response == 0 ? entity : null;
                }
            }
        }

        private Spectacle ExtractSpectacle(IDataRecord rs)
        {
            long id = rs.GetInt64(rs.GetOrdinal("ids"));
            string name = rs.GetString(rs.GetOrdinal("name"));
            string dateStr = rs.GetString(rs.GetOrdinal("start_date"));
            int duration = rs.GetInt32(rs.GetOrdinal("duration"));
            int capacity = rs.GetInt32(rs.GetOrdinal("capacity"));
            string location = rs.GetString(rs.GetOrdinal("location"));

            DateTime date = DateTime.ParseExact(dateStr, DateFormat, null);

            return new Spectacle(id, name, date, duration, capacity, location);
        }
    }
}