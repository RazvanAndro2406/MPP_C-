namespace ChatNetworking.JsonProtocol
{
    public enum ResponseType
    {
        // General status
        Ok, 
        Error, 
        Success,

        // Chat & Friends operations
        GetLoggedFriends, 
        Update, 
        NewMessage, 
        FriendLoggedIn, 
        FriendLoggedOut,
        
        // Notification for data updates
        DomainDataChanged,
        
        // List results
        ArtistsList, 
        SpectaclesList, 
        TicketsList, 
        ArtistSpectaclesList,
        BuyersList, 
        TicketSalesList,
        
        // Single entity results
        Artist, 
        Spectacle, 
        Ticket, 
        ArtistSpectacle,
        Buyer, 
        TicketSale,
        
        // Specific checks/values
        RelationExists, 
        IntegerValue
    }
}