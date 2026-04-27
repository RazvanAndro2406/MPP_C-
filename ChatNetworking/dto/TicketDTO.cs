using System;

namespace Ticketing.Networking.Dto
{
    [Serializable]
    public class TicketDto
    {
        public long? Id { get; set; }
        public double Price { get; set; }
        public long? SpectacleId { get; set; }

        public TicketDto() { }

        public TicketDto(long? id, double price, long? spectacleId)
        {
            Id = id;
            Price = price;
            SpectacleId = spectacleId;
        }

        public TicketDto(double price, long? spectacleId) : this(null, price, spectacleId) { }

        public override string ToString() => $"TicketDTO{{id={Id}, price={Price}, spectacleId={SpectacleId}}}";
    }
}