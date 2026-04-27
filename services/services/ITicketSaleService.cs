using System.Collections.Generic;
using Ticketing.Model.Domain;

namespace Ticketing.Services
{
    public interface ITicketSaleService
    {
        // Java: List<TicketSale> getTicketSalesByBuyer(Long buyerId);
        IList<TicketSale> GetTicketSalesByBuyer(long buyerId);

        // Java: List<TicketSale> getAllTicketSales();
        IList<TicketSale> GetAllTicketSales();

        // Java: TicketSale sellTicket(Long spectacleId, String buyerEmail, int seats);
        TicketSale SellTicket(long spectacleId, string buyerEmail, int seats);

        // Java: void increaseTicketSeats(Long ticketSaleId, int extraSeats);
        void IncreaseTicketSeats(long ticketSaleId, int extraSeats);

        // Java: int getSoldSeatsForSpectacle(Long spectacleId);
        int GetSoldSeatsForSpectacle(long spectacleId);
    }
}