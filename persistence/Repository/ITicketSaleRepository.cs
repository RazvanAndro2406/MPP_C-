using System.Collections.Generic;
using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository
{
    public interface ITicketSaleRepository : IRepository<long, TicketSale>
    {
        IEnumerable<TicketSale> FindByBuyerId(long buyerId);
        int GetSoldSeatsForSpectacle(long spectacleId);
    }
}