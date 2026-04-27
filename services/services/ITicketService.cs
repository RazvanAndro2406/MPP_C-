using System.Collections.Generic;
using Ticketing.Model.Domain;

namespace Ticketing.Services
{
    public interface ITicketService
    {
        // Java: List<Ticket> getAllTickets();
        IList<Ticket> GetAllTickets();

        // Java: void addTicket(double price, Long spectacleId);
        void AddTicket(double price, long spectacleId);

        // Java: void updateTicket(Ticket ticket);
        void UpdateTicket(Ticket ticket);

        // Java: void deleteTicket(Long id);
        void DeleteTicket(long id);

        // Java: Optional<Ticket> getTicketById(Long id);
        Ticket? GetTicketById(long id);

        // Java: List<Ticket> getTicketsBySpectacleId(Long spectacleId);
        IList<Ticket> GetTicketsBySpectacleId(long spectacleId);
    }
}