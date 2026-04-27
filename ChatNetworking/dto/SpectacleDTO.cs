using System;

namespace Ticketing.Networking.Dto
{
    [Serializable]
    public class SpectacleDto
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public int Duration { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }

        public SpectacleDto() { }

        public SpectacleDto(long? id, string name, DateTime startDate, int duration, int capacity, string location)
        {
            Id = id;
            Name = name;
            StartDate = startDate;
            Duration = duration;
            Capacity = capacity;
            Location = location;
        }

        public SpectacleDto(string name, DateTime startDate, int duration, int capacity, string location) 
            : this(null, name, startDate, duration, capacity, location) { }

        public override string ToString() => $"SpectacleDTO{{id={Id}, name='{Name}', start_date={StartDate}, duration={Duration}, capacity={Capacity}, location='{Location}'}}";
    }
}