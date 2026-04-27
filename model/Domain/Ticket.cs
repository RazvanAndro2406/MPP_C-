using System;

namespace Ticketing.Model.Domain
{
    [Serializable]
    public class Ticket : Entity<long>
    {
        private Spectacle? _spectacle;
        public double Price { get; set; }
        public long SpectacleId { get; set; }

        // Property with logic to keep SpectacleId in sync, matching Java's setSpectacle
        public Spectacle? Spectacle
        {
            get => _spectacle;
            set
            {
                _spectacle = value;
                if (_spectacle != null)
                {
                    SpectacleId = _spectacle.Id;
                }
            }
        }

        // Constructor 1: Matching public Ticket(Long id, double price, Long spectacleId)
        public Ticket(long id, double price, long spectacleId) : base(id)
        {
            Price = price;
            SpectacleId = spectacleId;
        }

        // Constructor 2: Matching public Ticket(double price, Long spectacleId)
        public Ticket(double price, long spectacleId) : base(0)
        {
            Price = price;
            SpectacleId = spectacleId;
        }

        // Constructor 3: Matching public Ticket(Long id, double price, Spectacle spectacle)
        public Ticket(long id, double price, Spectacle spectacle) : base(id)
        {
            Price = price;
            Spectacle = spectacle;
            SpectacleId = spectacle != null ? spectacle.Id : 0;
        }

        // Constructor 4: Parameterless (matches Java public Ticket())
        public Ticket() : base(0)
        {
            Price = 0;
            SpectacleId = 0;
        }

        public override string ToString()
        {
            return $"Ticket{{id={Id}, price={Price}, spectacle={(Spectacle != null ? Spectacle.Name : "N/A")}}}";
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not Ticket other) return false;

            return base.Equals(obj) &&
                   Math.Abs(Price - other.Price) < 0.0001 && // Standard double comparison in C#
                   SpectacleId == other.SpectacleId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Price, SpectacleId);
        }
    }
}