namespace Ticketing.Services
{
    /// <summary>
    /// Master interface that aggregates all domain-specific services 
    /// and networking capabilities into a single facade.
    /// </summary>
    public interface IServicesFacade : 
        IArtistService, 
        IArtistSpectacleService, 
        IBuyerService, 
        IChatServices, 
        ISpectacleService, 
        ITicketSaleService, 
        ITicketService
    {
        // This interface remains empty as it only serves to 
        // combine the inherited interfaces.
    }
}