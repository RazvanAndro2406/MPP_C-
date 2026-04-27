using System;

namespace Ticketing.Networking.Dto
{
    [Serializable]
    public class TicketSaleDto
    {
        public long? Id { get; set; }
        public long? SpectacleId { get; set; }
        public long? BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int Seats { get; set; }
        public DateTime SoldAt { get; set; }
        public string SpectacleName { get; set; }

        public TicketSaleDto() { }

        public TicketSaleDto(long? id, long? spectacleId, long? buyerId, string buyerName, int seats, DateTime soldAt, string spectacleName)
        {
            Id = id;
            SpectacleId = spectacleId;
            BuyerId = buyerId;
            BuyerName = buyerName;
            Seats = seats;
            SoldAt = soldAt;
            SpectacleName = spectacleName;
        }
    }
}