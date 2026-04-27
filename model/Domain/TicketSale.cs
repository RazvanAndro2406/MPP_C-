using System;

namespace Ticketing.Model.Domain
{
    [Serializable]
    public sealed class TicketSale : Entity<long>
    {
        // All attributes from Java
        public long SpectacleId { get; set; }
        public long BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int Seats { get; set; }
        public DateTime SoldAt { get; set; }
        public string SpectacleName { get; set; }

        // 1. Full Constructor (Matches the main Java constructor)
        public TicketSale(long id, long spectacleId, long buyerId, string buyerName, int seats, DateTime soldAt, string spectacleName) 
            : base(id)
        {
            SpectacleId = spectacleId;
            BuyerId = buyerId;
            BuyerName = buyerName;
            Seats = seats;
            SoldAt = soldAt;
            SpectacleName = spectacleName;
        }

        // 2. 5-Parameter Constructor (Matches Java's sid, bid, name, seats, date)
        public TicketSale(long spectacleId, long buyerId, string buyerName, int seats, DateTime soldAt) 
            : this(0, spectacleId, buyerId, buyerName, seats, soldAt, string.Empty)
        {
        }

        // 3. 6-Parameter Constructor (Matches Java's sid, bid, name, seats, date, sname)
        public TicketSale(long spectacleId, long buyerId, string buyerName, int seats, DateTime soldAt, string spectacleName) 
            : this(0, spectacleId, buyerId, buyerName, seats, soldAt, spectacleName)
        {
        }

        // Default constructor for serialization
        public TicketSale() : base(0) { }

        // 1:1 Equality Logic
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not TicketSale other) return false;
            if (!base.Equals(obj)) return false;

            return Seats == other.Seats &&
                   SpectacleId == other.SpectacleId &&
                   BuyerId == other.BuyerId &&
                   BuyerName == other.BuyerName &&
                   SoldAt == other.SoldAt;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), SpectacleId, BuyerId, BuyerName, Seats, SoldAt);
        }

        public override string ToString()
        {
            return $"TicketSale{{id={Id}, sid={SpectacleId}, bid={BuyerId}, seats={Seats}, sname='{SpectacleName}'}}";
        }
    }
}