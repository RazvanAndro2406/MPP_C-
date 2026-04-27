using System;
using System.Collections.Generic;
using Ticketing.Model.Domain;

namespace Ticketing.Services
{
    public interface INetworkProtocol
    {
        // --- Artist Operations ---
        IList<Artist> GetAllArtistsRemote();
        void AddArtistRemote(string name);
        void UpdateArtistRemote(Artist artist);
        void DeleteArtistRemote(long id);
        Artist? GetArtistByIdRemote(long id);

        // --- ArtistSpectacle Operations ---
        IList<ArtistSpectacle> GetAllArtistSpectaclesRemote();
        
        /// <summary> Sends a request to create a new relation between an artist and a spectacle. </summary>
        void AddArtistSpectacleRemote(long artistId, long spectacleId);

        /// <summary> Deletes a specific artist-spectacle relation by its unique ID. </summary>
        void DeleteArtistSpectacleRemote(long id);

        /// <summary> Finds a specific artist-spectacle relation by its ID. </summary>
        ArtistSpectacle? GetArtistSpectacleByIdRemote(long id);

        /// <summary> Retrieves all spectacles associated with a specific artist ID. </summary>
        IList<ArtistSpectacle> GetByArtistIdRemote(long artistId);

        /// <summary> Retrieves all artists associated with a specific spectacle ID. </summary>
        IList<ArtistSpectacle> GetBySpectacleIdRemote(long spectacleId);

        /// <summary> Deletes the relation link using the IDs of both the artist and the spectacle. </summary>
        void DeleteByArtistAndSpectacleRemote(long artistId, long spectacleId);

        /// <summary> Checks the server to see if a relation already exists between an artist and a spectacle. </summary>
        bool ExistsRelationRemote(long artistId, long spectacleId);

        // --- Buyer Operations ---
        Buyer GetOrCreateBuyerRemote(string email, string name);
        Buyer? GetBuyerByEmailRemote(string email);
        Buyer GetOrCreateBuyerRemote(long buyerId, string buyerName);

        /// <summary> Fetches a specific buyer by their primary key. </summary>
        Buyer? GetBuyerByIdRemote(long id);

        /// <summary> Fetches the complete list of buyers from the database. </summary>
        IList<Buyer> GetAllBuyersRemote();

        /// <summary> Removes a buyer record via the network protocol. </summary>
        void DeleteBuyerRemote(long id);

        // --- Spectacle Operations ---
        IList<Spectacle> GetAllSpectaclesRemote();

        /// <summary> Sends parameters to create a new spectacle record on the server. </summary>
        void AddSpectacleRemote(string name, DateTime date, int duration, int capacity, string location);

        /// <summary> Sends a full Spectacle object to be updated in the database. </summary>
        void UpdateSpectacleRemote(Spectacle spectacle);

        /// <summary> Deletes a spectacle by its ID over the network. </summary>
        void DeleteSpectacleRemote(long id);

        /// <summary> Requests a specific spectacle by ID. </summary>
        Spectacle? GetSpectacleByIdRemote(long id);

        // --- TicketSale Operations ---
        TicketSale SellTicketRemote(long spectacleId, string buyerEmail, int seats);
        IList<TicketSale> GetTicketSalesByBuyerRemote(long buyerId);

        /// <summary> Fetches every ticket sale record in the system. </summary>
        IList<TicketSale> GetAllTicketSalesRemote();

        /// <summary> Updates an existing sale to include more seats. </summary>
        void IncreaseTicketSeatsRemote(long ticketSaleId, int extraSeats);

        /// <summary> Queries the current occupancy for a specific spectacle. </summary>
        int GetSoldSeatsForSpectacleRemote(long spectacleId);

        // --- Ticket Operations ---
        IList<Ticket> GetAllTicketsRemote();

        /// <summary> Creates a new ticket category with a specific price for a spectacle. </summary>
        void AddTicketRemote(double price, long spectacleId);

        /// <summary> Updates the details of an existing ticket record. </summary>
        void UpdateTicketRemote(Ticket ticket);

        /// <summary> Removes a ticket record from the system over the network. </summary>
        void DeleteTicketRemote(long id);

        /// <summary> Fetches a specific ticket by its primary ID. </summary>
        Ticket? GetTicketByIdRemote(long id);

        /// <summary> Retrieves all ticket types available for a specific spectacle. </summary>
        IList<Ticket> GetTicketsBySpectacleIdRemote(long spectacleId);
    }
}