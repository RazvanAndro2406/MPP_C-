using System;
using Org.Example.Domain;

namespace Lab2.domain
{
    public class Spectacle : Entity<long?>
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public int Duration { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }

        public Spectacle(long? id) : base(id)
        {
        }

        public Spectacle(long? id, string name, DateTime startDate, int duration, int capacity, string location) 
            : base(id)
        {
            Name = name;
            StartDate = startDate;
            Duration = duration;
            Capacity = capacity;
            Location = location;
        }

        public Spectacle(string name, DateTime startDate, int duration, int capacity, string location) 
            : base(null)
        {
            Name = name;
            StartDate = startDate;
            Duration = duration;
            Capacity = capacity;
            Location = location;
        }

        public override string ToString()
        {
            return $"Spectacle{{Name='{Name}', StartDate={StartDate}, Duration={Duration}, Capacity={Capacity}, Location='{Location}'}}";
        }
    }
}