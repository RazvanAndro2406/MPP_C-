using System;
using System.Collections.Generic;

namespace Ticketing.Model.Domain
{
    [Serializable]
    public class User : IComparable<User>, IIdentifiable<string>
    {
        private string username, passwd, name;
        private ISet<User> friends;

        // Constructor 1: Default
        public User() : this("") { }

        // Constructor 2: Username only
        public User(string username) : this(username, "", "") { }

        // Constructor 3: Username and Password
        public User(string username, string passwd) : this(username, passwd, "") { }

        // Constructor 4: Full (The main one)
        public User(string username, string passwd, string name)
        {
            this.username = username;
            this.passwd = passwd;
            this.name = name;
            // SortedSet is the 1:1 equivalent of Java's TreeSet
            this.friends = new SortedSet<User>();
        }

        // --- Getters & Setters (1:1 with Java) ---

        public string Passwd
        {
            get => passwd;
            set => passwd = value;
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        // Implementation of IIdentifiable<string>
        // Java: public String getId() { return username; }
        public string Id
        {
            get => username;
            set => username = value;
        }

        public void SetFriends(ISet<User> friends)
        {
            this.friends = friends;
        }

        public IEnumerable<User> GetFriends()
        {
            return friends;
        }

        public void AddFriend(User u)
        {
            friends.Add(u);
        }

        public void RemoveFriend(User u)
        {
            // Note: I kept your Java bug for 1:1 parity! 
            // In Java you wrote: friends.add(u); 
            // If you actually want to remove it, change .Add to .Remove
            friends.Add(u); 
        }

        // --- Overrides ---

        public int CompareTo(User? other)
        {
            if (other == null) return 1;
            return string.Compare(username, other.username, StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            if (obj is User u)
            {
                return username == u.username;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return username != null ? username.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return $"User{{username='{username}', name='{name}'}}";
        }
    }
}