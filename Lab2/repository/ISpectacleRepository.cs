using Lab2.domain;
using org.example.repository;

namespace Lab2.repository;

public interface ISpectacleRepository:IRepository<long, Spectacle>
{
    public bool DeleteByName(string name);
    public int GetAvailableSeats(long spectacleId);
    public bool SellTickets(long spectacleId, long buyerId, string buyerName, int numberOfSeats);
    public IEnumerable<TicketSale> GetTicketSalesByBuyer(long buyerId);
    public bool IncreaseTicketSeats(long ticketSaleId, int extraSeats);
}