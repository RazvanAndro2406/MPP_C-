using System.Data;
using Lab2.domain;
using Microsoft.Data.Sqlite;
using org.example.repository;

namespace Lab2.repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public User? Authenticate(string username, string password)
        {
            const string sql = "SELECT id, username, password FROM users WHERE username = @username AND password = @password";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        return ExtractUser(reader);
                    }
                }
            }
        }

        public User FindOne(long id)
        {
            const string sql = "SELECT id, username, password FROM users WHERE id = @id";

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
                            return ExtractUser(reader);
                        }
                    }
                }
            }

            return null;
        }

        public IEnumerable<User> FindAll()
        {
            var users = new List<User>();
            const string sql = "SELECT id, username, password FROM users";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(ExtractUser(reader));
                    }
                }
            }

            return users;
        }

        public User Save(User entity)
        {
            const string sql = "INSERT INTO users (username, password) VALUES (@username, @password); SELECT last_insert_rowid();";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@username", entity.Username);
                    command.Parameters.AddWithValue("@password", entity.Password);
                    entity.Id = (long)command.ExecuteScalar();
                    return entity;
                }
            }
        }

        public bool Delete(long id)
        {
            const string sql = "DELETE FROM users WHERE id = @id";

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

        public User Update(User entity)
        {
            const string sql = "UPDATE users SET username = @username, password = @password WHERE id = @id";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", entity.Id);
                    command.Parameters.AddWithValue("@username", entity.Username);
                    command.Parameters.AddWithValue("@password", entity.Password);

                    var rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected == 0 ? entity : null;
                }
            }
        }

        private static User ExtractUser(IDataRecord reader)
        {
            var id = reader.GetInt64(reader.GetOrdinal("id"));
            var username = reader.GetString(reader.GetOrdinal("username"));
            var password = reader.GetString(reader.GetOrdinal("password"));
            return new User(id, username, password);
        }
    }
}

