using System;

namespace Ticketing.Model.Domain
{
    [Serializable]
    public sealed class Buyer : Entity<long>
    {
        public string Name { get; set; }
        public string Email { get; set; }

        // Constructor with ID
        public Buyer(long id, string name, string email) : base(id)
        {
            Name = name;
            Email = email;
        }

        // Constructor for new entities (ID defaults to 0 or null depending on your Entity implementation)
        public Buyer(string name, string email) : base(0)
        {
            Name = name;
            Email = email;
        }

        // Parameterless constructor
        public Buyer() : base(0) { }

        public override string ToString()
        {
            return $"Buyer{{id={Id}, name='{Name}', email='{Email}'}}";
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not Buyer other) return false;
            
            // Assuming Entity<long> handles the ID comparison in its own Equals
            return base.Equals(obj) && 
                   Name == other.Name && 
                   Email == other.Email;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Name, Email);
        }
    }
}