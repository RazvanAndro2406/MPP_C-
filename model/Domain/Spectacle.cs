using System;

namespace Ticketing.Model.Domain
{
    [Serializable]
    public class Spectacle : Entity<long>
    {
        // 1:1 Mapping of properties
        public string Name { get; set; }
        public DateTime Start_date { get; set; } // Matches Java start_date
        public int Duration { get; set; }       // Matches Java duration
        public int Capacity { get; set; }
        public string Location { get; set; }

        // Constructor 1: Full initialization (matches Java)
        public Spectacle(long id, string name, DateTime start_date, int duration, int capacity, string location) 
            : base(id)
        {
            Name = name;
            Start_date = start_date;
            Duration = duration;
            Capacity = capacity;
            Location = location;
        }

        // Constructor 2: Without ID (matches Java super(null))
        public Spectacle(string name, DateTime start_date, int duration, int capacity, string location) 
            : this(0, name, start_date, duration, capacity, location)
        {
        }

        // Constructor 3: ID only
        public Spectacle(long id) : base(id)
        {
            Name = string.Empty;
            Location = string.Empty;
        }

        // Constructor 4: Parameterless (matches Java super(null))
        public Spectacle() : base(0)
        {
            Name = string.Empty;
            Location = string.Empty;
        }

        // 1:1 Equality logic
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not Spectacle other) return false;
            if (!base.Equals(obj)) return false;

            return Duration == other.Duration &&
                   Capacity == other.Capacity &&
                   Name == other.Name &&
                   Start_date == other.Start_date &&
                   Location == other.Location;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Name, Start_date, Duration, Capacity, Location);
        }

        public override string ToString()
        {
            return $"Spectacle{{id={Id}, name='{Name}', start_date={Start_date}, duration={Duration}, capacity={Capacity}, location='{Location}'}}";
        }
    }
}