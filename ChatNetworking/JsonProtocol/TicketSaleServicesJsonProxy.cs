
using Ticketing.Model.Domain;
using Ticketing.Services;

namespace ChatNetworking.JsonProtocol
{
    public class TicketSaleServicesJsonProxy : ITicketSaleService
    {
        private readonly INetworkProtocol _delegate;

        public TicketSaleServicesJsonProxy(INetworkProtocol networkDelegate)
        {
            _delegate = networkDelegate;
        }

        // Java: public List<TicketSale> getTicketSalesByBuyer(Long buyerId)
        public IList<TicketSale> GetTicketSalesByBuyer(long buyerId)
        {
            return _delegate.GetTicketSalesByBuyerRemote(buyerId);
        }

        // Java: public List<TicketSale> getAllTicketSales()
        public IList<TicketSale> GetAllTicketSales()
        {
            return _delegate.GetAllTicketSalesRemote();
        }

        // Java: public TicketSale sellTicket(Long spectacleId, String buyerEmail, int seats)
        public TicketSale SellTicket(long spectacleId, string buyerEmail, int seats)
        {
            return _delegate.SellTicketRemote(spectacleId, buyerEmail, seats);
        }

        // Java: public void increaseTicketSeats(Long ticketSaleId, int extraSeats)
        public void IncreaseTicketSeats(long ticketSaleId, int extraSeats)
        {
            _delegate.IncreaseTicketSeatsRemote(ticketSaleId, extraSeats);
        }

        // Java: public int getSoldSeatsForSpectacle(Long spectacleId)
        public int GetSoldSeatsForSpectacle(long spectacleId)
        {
            return _delegate.GetSoldSeatsForSpectacleRemote(spectacleId);
        }
    }
}