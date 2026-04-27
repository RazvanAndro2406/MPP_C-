namespace ChatNetworking.JsonProtocol
{
    public enum RequestType
    {
        // Chat operations
        Login, 
        Logout, 
        GetLoggedFriends, 
        SendMessage,
        
        // Artist operations
        GetAllArtists, 
        GetArtist, 
        AddArtist, 
        UpdateArtist, 
        DeleteArtist,
        
        // Spectacle operations
        GetAllSpectacles, 
        GetSpectacle, 
        AddSpectacle, 
        UpdateSpectacle, 
        DeleteSpectacle,
        
        // Ticket operations
        GetAllTickets, 
        GetTicket, 
        AddTicket, 
        UpdateTicket, 
        DeleteTicket, 
        GetTicketsBySpectacle,
        
        // ArtistSpectacle operations
        GetAllArtistSpectacles, 
        GetArtistSpectacle, 
        AddArtistSpectacle, 
        DeleteArtistSpectacle,
        GetByArtistId, 
        GetBySpectacleId, 
        DeleteByArtistAndSpectacle, 
        ExistsRelation,

        // Buyer operations
        GetAllBuyers, 
        GetBuyer, 
        GetOrCreateBuyer, 
        DeleteBuyer,
        GetBuyerByEmail,

        // TicketSale operations
        GetAllTicketSales, 
        GetTicketSalesByBuyer, 
        SellTicket, 
        IncreaseTicketSeats, 
        GetSoldSeatsForSpectacle
    }
}