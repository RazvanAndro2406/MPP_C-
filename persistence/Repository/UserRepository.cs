using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using Microsoft.Data.Sqlite;
using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository
{
    public sealed class UserRepository : IUserRepository
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UserRepository));
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            // Java: logger.info("Initializing UserRepository");
            Logger.Info("Initializing UserRepository");
            _connectionString = connectionString;
        }

        // Matches Java: User findBy(String username, String passwd)
        public User? FindBy(string username, string password)
        {
            // Note: Java version didn't select 'id', but C# Entity needs it for tracking. 
            // We select username/password to match your Java query exactly.
            const string sql = "SELECT username, password FROM users WHERE username = @username AND password = @password";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Matches Java: return new User(resultSet.getString("username"), resultSet.getString("password"));
                            return new User(
                                reader.GetString(reader.GetOrdinal("username")),
                                reader.GetString(reader.GetOrdinal("password"))
                            );
                        }
                    }
                }
            }
            return null;
        }

        // Matches Java: Iterable<User> getFriendsOf(User user)
        public IEnumerable<User> GetFriendsOf(User user)
        {
            var friends = new List<User>();
            // Matches Java SQL: SELECT username, password FROM users WHERE username <> ?
            const string sql = "SELECT username, password FROM users WHERE username <> @id";

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    // Matches Java: statement.setString(1, user.getId());
                    // Assuming user.Id returns the username/string ID as per your Java snippet
                    command.Parameters.AddWithValue("@id", user.Id.ToString());

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            friends.Add(new User(
                                reader.GetString(reader.GetOrdinal("username")),
                                reader.GetString(reader.GetOrdinal("password"))
                            ));
                        }
                    }
                }
            }
            return friends;
        }

        /* The CRUD methods below (FindOne, FindAll, Save, Delete, Update) 
           are usually required by IRepository. Keep them if needed, 
           but the two above are the 1:1 conversions from your specific file.
        */

        public User? FindOne(long id) { /* logic */ return null; }
        public IEnumerable<User> FindAll() { /* logic */ return new List<User>(); }
        public User Save(User entity) { /* logic */ return entity; }
        public bool Delete(long id) { /* logic */ return false; }
        public User? Update(User entity) { /* logic */ return entity; }
    }
}