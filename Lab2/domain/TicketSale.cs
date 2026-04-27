using Org.Example.Domain;

namespace Lab2.domain
{
    public class TicketSale : Entity<long?>
    {
        public long SpectacleId { get; set; }
        public long BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int Seats { get; set; }
        public DateTime SoldAt { get; set; }
        public string SpectacleName { get; set; }

        public TicketSale(long? id, long spectacleId, long buyerId, string buyerName, int seats, DateTime soldAt, string spectacleName)
            : base(id)
        {
            SpectacleId = spectacleId;
            BuyerId = buyerId;
            BuyerName = buyerName;
            Seats = seats;
            SoldAt = soldAt;
            SpectacleName = spectacleName;
        }
    }
}

