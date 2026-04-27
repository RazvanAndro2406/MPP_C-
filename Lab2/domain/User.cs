using Org.Example.Domain;

namespace Lab2.domain
{
    public class User : Entity<long>
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public User(long id, string username, string password) : base(id)
        {
            Username = username;
            Password = password;
        }

        public User(string username, string password) : base(default)
        {
            Username = username;
            Password = password;
        }
    }
}
