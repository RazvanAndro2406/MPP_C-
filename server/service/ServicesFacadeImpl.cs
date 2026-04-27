using System;
using System.Collections.Generic;
using Ticketing.Model.Domain;
using Ticketing.Services;

namespace Ticketing.Service
{
    public class ServicesFacadeImpl : IServicesFacade
    {
        private readonly IArtistService _artistService;
        private readonly IArtistSpectacleService _artistSpectacleService;
        private readonly IBuyerService _buyerService;
        private readonly IChatServices _chatServices;
        private readonly ISpectacleService _spectacleService;
        private readonly ITicketSaleService _ticketSaleService;
        private readonly ITicketService _ticketService;

        public ServicesFacadeImpl(IArtistService artistService,
                              IArtistSpectacleService artistSpectacleService,
                              IBuyerService buyerService,
                              IChatServices chatServices,
                              ISpectacleService spectacleService,
                              ITicketSaleService ticketSaleService,
                              ITicketService ticketService)
        {
            _artistService = artistService;
            _artistSpectacleService = artistSpectacleService;
            _buyerService = buyerService;
            _chatServices = chatServices;
            _spectacleService = spectacleService;
            _ticketSaleService = ticketSaleService;
            _ticketService = ticketService;
        }

        // --- IArtistService Delegation ---
        public IList<Artist> GetAllArtists() => _artistService.GetAllArtists();
        public void AddArtist(string name) => _artistService.AddArtist(name);
        public void UpdateArtist(Artist artist) => _artistService.UpdateArtist(artist);
        public void DeleteArtist(long id) => _artistService.DeleteArtist(id);
        public Artist? GetArtistById(long id) => _artistService.GetArtistById(id);

        // --- IArtistSpectacleService Delegation ---
        public IList<ArtistSpectacle> GetAllArtistSpectacles() => _artistSpectacleService.GetAllArtistSpectacles();
        public void AddArtistSpectacle(long artistId, long spectacleId) => _artistSpectacleService.AddArtistSpectacle(artistId, spectacleId);
        public void DeleteArtistSpectacle(long id) => _artistSpectacleService.DeleteArtistSpectacle(id);
        public ArtistSpectacle? GetArtistSpectacleById(long id) => _artistSpectacleService.GetArtistSpectacleById(id);
        public IList<ArtistSpectacle> GetArtistSpectacleByArtistId(long artistId) => _artistSpectacleService.GetArtistSpectacleByArtistId(artistId);
        public IList<ArtistSpectacle> GetArtistSpectacleBySpectacleId(long spectacleId) => _artistSpectacleService.GetArtistSpectacleBySpectacleId(spectacleId);
        public void DeleteByArtistAndSpectacle(long artistId, long spectacleId) => _artistSpectacleService.DeleteByArtistAndSpectacle(artistId, spectacleId);
        public bool ExistsRelationBetweenArtistAndSpectacle(long artistId, long spectacleId) => _artistSpectacleService.ExistsRelationBetweenArtistAndSpectacle(artistId, spectacleId);

        // --- IBuyerService Delegation ---
        public Buyer GetOrCreateBuyer(string email, string name) => _buyerService.GetOrCreateBuyer(email, name);
        public Buyer? GetBuyerById(long id) => _buyerService.GetBuyerById(id);
        public Buyer? GetBuyerByEmail(string email) => _buyerService.GetBuyerByEmail(email);
        public IList<Buyer> GetAllBuyers() => _buyerService.GetAllBuyers();
        public void DeleteBuyer(long id) => _buyerService.DeleteBuyer(id);

        // --- IChatServices Delegation ---
        public void Login(User user, IChatObserver client) => _chatServices.Login(user, client);
        public void SendMessage(Message message) => _chatServices.SendMessage(message);
        public void Logout(User user, IChatObserver client) => _chatServices.Logout(user, client);
        public User[] GetLoggedFriends(User user) => _chatServices.GetLoggedFriends(user);
        
        // This maps to IDomainUpdatePublisher via the IChatServices implementation
        public void DomainDataChanged(string entityType) => _chatServices.DomainDataChanged(entityType);

        // --- ISpectacleService Delegation ---
        public IList<Spectacle> GetAllSpectacles() => _spectacleService.GetAllSpectacles();
        public void AddSpectacle(string name, DateTime date, int duration, int capacity, string location) => _spectacleService.AddSpectacle(name, date, duration, capacity, location);
        public void UpdateSpectacle(Spectacle spectacle) => _spectacleService.UpdateSpectacle(spectacle);
        public void DeleteSpectacle(long id) => _spectacleService.DeleteSpectacle(id);
        public Spectacle? GetSpectacleById(long id) => _spectacleService.GetSpectacleById(id);

        // --- ITicketSaleService Delegation ---
        public IList<TicketSale> GetTicketSalesByBuyer(long buyerId) => _ticketSaleService.GetTicketSalesByBuyer(buyerId);
        public IList<TicketSale> GetAllTicketSales() => _ticketSaleService.GetAllTicketSales();
        public TicketSale SellTicket(long spectacleId, string buyerEmail, int seats) => _ticketSaleService.SellTicket(spectacleId, buyerEmail, seats);
        public void IncreaseTicketSeats(long ticketSaleId, int extraSeats) => _ticketSaleService.IncreaseTicketSeats(ticketSaleId, extraSeats);
        public int GetSoldSeatsForSpectacle(long spectacleId) => _ticketSaleService.GetSoldSeatsForSpectacle(spectacleId);

        // --- ITicketService Delegation ---
        public IList<Ticket> GetAllTickets() => _ticketService.GetAllTickets();
        public void AddTicket(double price, long spectacleId) => _ticketService.AddTicket(price, spectacleId);
        public void UpdateTicket(Ticket ticket) => _ticketService.UpdateTicket(ticket);
        public void DeleteTicket(long id) => _ticketService.DeleteTicket(id);
        public Ticket? GetTicketById(long id) => _ticketService.GetTicketById(id);
        public IList<Ticket> GetTicketsBySpectacleId(long spectacleId) => _ticketService.GetTicketsBySpectacleId(spectacleId);
    }
}