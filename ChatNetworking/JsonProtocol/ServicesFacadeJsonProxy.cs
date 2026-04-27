
using Ticketing.Model.Domain;
using Ticketing.Services;

namespace ChatNetworking.JsonProtocol
{
    public class ServicesFacadeJsonProxy : IServicesFacade
    {
        private readonly IArtistService _artistServiceProxy;
        private readonly IArtistSpectacleService _artistSpectacleServiceProxy;
        private readonly IBuyerService _buyerServiceProxy;
        private readonly IChatServices _chatServicesProxy;
        private readonly ISpectacleService _spectacleServiceProxy;
        private readonly ITicketSaleService _ticketSaleServiceProxy;
        private readonly ITicketService _ticketServiceProxy;

        public ServicesFacadeJsonProxy(INetworkProtocol network)
        {
            // Initialize all specific proxies using the shared network protocol
            _artistServiceProxy = new ArtistServicesJsonProxy(network);
            _spectacleServiceProxy = new SpectacleServicesJsonProxy(network);
            _ticketServiceProxy = new TicketServicesJsonProxy(network);
            _buyerServiceProxy = new BuyerServicesJsonProxy(network);
            _ticketSaleServiceProxy = new TicketSaleServicesJsonProxy(network);
            _artistSpectacleServiceProxy = new ArtistSpectacleServicesJsonProxy(network);
            
            // The network protocol itself implements IChatServices in our architecture
            _chatServicesProxy = (IChatServices)network;
        }

        // --- Artist Service Delegation ---

        public IList<Artist> GetAllArtists() => _artistServiceProxy.GetAllArtists();
        public void AddArtist(string name) => _artistServiceProxy.AddArtist(name);
        public void UpdateArtist(Artist artist) => _artistServiceProxy.UpdateArtist(artist);
        public void DeleteArtist(long id) => _artistServiceProxy.DeleteArtist(id);
        public Artist? GetArtistById(long id) => _artistServiceProxy.GetArtistById(id);

        // --- ArtistSpectacle Service Delegation ---

        public IList<ArtistSpectacle> GetAllArtistSpectacles() => _artistSpectacleServiceProxy.GetAllArtistSpectacles();
        public void AddArtistSpectacle(long artistId, long spectacleId) => _artistSpectacleServiceProxy.AddArtistSpectacle(artistId, spectacleId);
        public void DeleteArtistSpectacle(long id) => _artistSpectacleServiceProxy.DeleteArtistSpectacle(id);
        public ArtistSpectacle? GetArtistSpectacleById(long id) => _artistSpectacleServiceProxy.GetArtistSpectacleById(id);
        public IList<ArtistSpectacle> GetArtistSpectacleByArtistId(long artistId) => _artistSpectacleServiceProxy.GetArtistSpectacleByArtistId(artistId);
        public IList<ArtistSpectacle> GetArtistSpectacleBySpectacleId(long spectacleId) => _artistSpectacleServiceProxy.GetArtistSpectacleBySpectacleId(spectacleId);
        public void DeleteByArtistAndSpectacle(long artistId, long spectacleId) => _artistSpectacleServiceProxy.DeleteByArtistAndSpectacle(artistId, spectacleId);
        public bool ExistsRelationBetweenArtistAndSpectacle(long artistId, long spectacleId) => _artistSpectacleServiceProxy.ExistsRelationBetweenArtistAndSpectacle(artistId, spectacleId);

        // --- Buyer Service Delegation ---

        public Buyer GetOrCreateBuyer(string buyerEmail, string buyerName) => _buyerServiceProxy.GetOrCreateBuyer(buyerEmail, buyerName);
        public Buyer? GetBuyerById(long id) => _buyerServiceProxy.GetBuyerById(id);
        public Buyer? GetBuyerByEmail(string email) => _buyerServiceProxy.GetBuyerByEmail(email);
        public IList<Buyer> GetAllBuyers() => _buyerServiceProxy.GetAllBuyers();
        public void DeleteBuyer(long id) => _buyerServiceProxy.DeleteBuyer(id);

        // --- Chat Services Delegation ---

        public void Login(User user, IChatObserver client) => _chatServicesProxy.Login(user, client);
        public void SendMessage(Message message) => _chatServicesProxy.SendMessage(message);
        public void Logout(User user, IChatObserver client) => _chatServicesProxy.Logout(user, client);
        public User[] GetLoggedFriends(User user) => _chatServicesProxy.GetLoggedFriends(user);
        public void DomainDataChanged(string entityType) => _chatServicesProxy.DomainDataChanged(entityType);

        // --- Spectacle Service Delegation ---

        public IList<Spectacle> GetAllSpectacles() => _spectacleServiceProxy.GetAllSpectacles();
        public void AddSpectacle(string name, DateTime date, int duration, int capacity, string location) => _spectacleServiceProxy.AddSpectacle(name, date, duration, capacity, location);
        public void UpdateSpectacle(Spectacle spectacle) => _spectacleServiceProxy.UpdateSpectacle(spectacle);
        public void DeleteSpectacle(long id) => _spectacleServiceProxy.DeleteSpectacle(id);
        public Spectacle? GetSpectacleById(long id) => _spectacleServiceProxy.GetSpectacleById(id);

        // --- TicketSale Service Delegation ---

        public IList<TicketSale> GetTicketSalesByBuyer(long buyerId) => _ticketSaleServiceProxy.GetTicketSalesByBuyer(buyerId);
        public IList<TicketSale> GetAllTicketSales() => _ticketSaleServiceProxy.GetAllTicketSales();
        public TicketSale SellTicket(long spectacleId, string buyerEmail, int seats) => _ticketSaleServiceProxy.SellTicket(spectacleId, buyerEmail, seats);
        public void IncreaseTicketSeats(long ticketSaleId, int extraSeats) => _ticketSaleServiceProxy.IncreaseTicketSeats(ticketSaleId, extraSeats);
        public int GetSoldSeatsForSpectacle(long spectacleId) => _ticketSaleServiceProxy.GetSoldSeatsForSpectacle(spectacleId);

        // --- Ticket Service Delegation ---

        public IList<Ticket> GetAllTickets() => _ticketServiceProxy.GetAllTickets();
        public void AddTicket(double price, long spectacleId) => _ticketServiceProxy.AddTicket(price, spectacleId);
        public void UpdateTicket(Ticket ticket) => _ticketServiceProxy.UpdateTicket(ticket);
        public void DeleteTicket(long id) => _ticketServiceProxy.DeleteTicket(id);
        public Ticket? GetTicketById(long id) => _ticketServiceProxy.GetTicketById(id);
        public IList<Ticket> GetTicketsBySpectacleId(long spectacleId) => _ticketServiceProxy.GetTicketsBySpectacleId(spectacleId);
    }
}