using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository
{
    public interface ITicketRepository : IRepository<long, Ticket>
    {
        // No extra methods needed for 1:1 parity
    }
}