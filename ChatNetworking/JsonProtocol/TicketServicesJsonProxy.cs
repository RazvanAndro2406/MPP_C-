
using Ticketing.Model.Domain;
using Ticketing.Services;

namespace ChatNetworking.JsonProtocol
{
    public class TicketServicesJsonProxy : ITicketService
    {
        private readonly INetworkProtocol _delegate;

        public TicketServicesJsonProxy(INetworkProtocol networkDelegate)
        {
            _delegate = networkDelegate;
        }

        // Java: public List<Ticket> getAllTickets()
        public IList<Ticket> GetAllTickets()
        {
            return _delegate.GetAllTicketsRemote();
        }

        // Java: public void addTicket(double price, Long spectacleId)
        public void AddTicket(double price, long spectacleId)
        {
            _delegate.AddTicketRemote(price, spectacleId);
        }

        // Java: public void updateTicket(Ticket ticket)
        public void UpdateTicket(Ticket ticket)
        {
            _delegate.UpdateTicketRemote(ticket);
        }

        // Java: public void deleteTicket(Long id)
        public void DeleteTicket(long id)
        {
            _delegate.DeleteTicketRemote(id);
        }

        // Java: public Optional<Ticket> getTicketById(Long id)
        public Ticket? GetTicketById(long id)
        {
            return _delegate.GetTicketByIdRemote(id);
        }

        // Java: public List<Ticket> getTicketsBySpectacleId(Long spectacleId)
        public IList<Ticket> GetTicketsBySpectacleId(long spectacleId)
        {
            return _delegate.GetTicketsBySpectacleIdRemote(spectacleId);
        }
    }
}