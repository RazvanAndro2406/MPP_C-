
using Ticketing.Networking.Dto;

namespace ChatNetworking.JsonProtocol
{
    [Serializable]
    public class Request
    {
        // Request Metadata
        public RequestType Type { get; set; }

        // Chat & User DTOs
        public UserDto User { get; set; }
        public MessageDto Message { get; set; }
        public UserDto[] Friends { get; set; }

        // Domain Specific DTOs
        public ArtistDto Artist { get; set; }
        public ArtistDto[] Artists { get; set; }
        public SpectacleDto Spectacle { get; set; }
        public SpectacleDto[] Spectacles { get; set; }
        public TicketDto Ticket { get; set; }
        public TicketDto[] Tickets { get; set; }
        public ArtistSpectacleDto ArtistSpectacle { get; set; }
        public ArtistSpectacleDto[] ArtistSpectacles { get; set; }
        public BuyerDto Buyer { get; set; }
        public BuyerDto[] Buyers { get; set; }
        public TicketSaleDto TicketSale { get; set; }
        public TicketSaleDto[] TicketSales { get; set; }

        // Operation Parameters (Nullable to match Java Long/Integer/Double)
        public long? Id { get; set; }
        public long? ArtistId { get; set; }
        public long? SpectacleId { get; set; }
        public long? BuyerId { get; set; }
        public string ArtistName { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public double? TicketPrice { get; set; }
        public int? Seats { get; set; }
        public int? ExtraSeats { get; set; }

        // Java: public Request() {}
        public Request() { }

        // 1:1 Match with Java toString() logic
        public override string ToString()
        {
            return $"Request{{" +
                   $"type={Type}, " +
                   $"buyerEmail='{BuyerEmail}', " +
                   $"buyerName='{BuyerName}', " +
                   $"id={Id}, " +
                   $"user={User}, " +
                   $"artist={Artist}, " +
                   $"spectacle={Spectacle}" +
                   $"}}";
        }
    }
}