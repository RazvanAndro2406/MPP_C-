using System;
using System.Collections.Generic;
using System.Linq;
using Ticketing.Model.Domain;
using Ticketing.Persistence.Repository;
using Ticketing.Services;

namespace Ticketing.Service
{
    public class TicketSaleServiceImpl : ITicketSaleService
    {
        private readonly ITicketSaleRepository _ticketSaleRepository;
        private readonly IBuyerRepository _buyerRepository;
        private readonly ISpectacleRepository _spectacleRepository;

        public TicketSaleServiceImpl(ITicketSaleRepository ticketSaleRepository,
                                 IBuyerRepository buyerRepository,
                                 ISpectacleRepository spectacleRepository)
        {
            _ticketSaleRepository = ticketSaleRepository;
            _buyerRepository = buyerRepository;
            _spectacleRepository = spectacleRepository;
        }

        // Java: public List<TicketSale> getTicketSalesByBuyer(Long buyerId)
        public IList<TicketSale> GetTicketSalesByBuyer(long buyerId)
        {
            return _ticketSaleRepository.FindByBuyerId(buyerId).ToList();
        }

        // Java: public List<TicketSale> getAllTicketSales()
        public IList<TicketSale> GetAllTicketSales()
        {
            return _ticketSaleRepository.FindAll().ToList();
        }

        // Java: public TicketSale sellTicket(Long spectacleId, String buyerEmail, int seats)
        public TicketSale SellTicket(long spectacleId, string buyerEmail, int seats)
        {
            // 1. Validation & fetching Spectacle details
            Spectacle? spectacle = _spectacleRepository.FindOne(spectacleId);
            if (spectacle == null)
            {
                throw new ArgumentException("Spectacolul nu exista.");
            }

            if (GetSoldSeatsForSpectacle(spectacleId) + seats > spectacle.Capacity)
            {
                throw new InvalidOperationException("Nu mai sunt suficiente locuri!");
            }

            // 2. Get or Create Buyer
            string normalizedEmail = buyerEmail.Trim().ToLower();
            Buyer? buyer = _buyerRepository.FindByEmail(normalizedEmail);

            if (buyer == null)
            {
                // Logic: Extract default name from email
                string defaultName = normalizedEmail.Split('@')[0];
                buyer = new Buyer(0, defaultName, normalizedEmail);
                _buyerRepository.Save(buyer);
            }

            // 3. Create TicketSale using model's constructor
            // Note: 0 passed as the entity ID for the new record
            TicketSale sale = new TicketSale(
                0,                          // id
                spectacle.Id,               // spectacleId
                buyer.Id,                   // buyerId
                buyer.Name,                 // buyerName
                seats,                      // seats
                DateTime.Now,               // soldAt
                spectacle.Name              // spectacleName
            );

            // 4. Save the Sale
            _ticketSaleRepository.Save(sale);

            return sale;
        }

        // Java: public void increaseTicketSeats(Long ticketSaleId, int extraSeats)
        public void IncreaseTicketSeats(long ticketSaleId, int extraSeats)
        {
            if (ticketSaleId <= 0)
            {
                throw new ArgumentException("Bilet invalid.");
            }
            if (extraSeats <= 0)
            {
                throw new ArgumentException("Locurile suplimentare trebuie sa fie pozitive.");
            }

            TicketSale? sale = _ticketSaleRepository.FindOne(ticketSaleId);
            if (sale == null)
            {
                throw new ArgumentException("Biletul nu exista.");
            }

            Spectacle? spectacle = _spectacleRepository.FindOne(sale.SpectacleId);
            if (spectacle == null)
            {
                throw new ArgumentException("Spectacolul nu exista.");
            }

            int soldSeats = _ticketSaleRepository.GetSoldSeatsForSpectacle(sale.SpectacleId);
            if (soldSeats + extraSeats > spectacle.Capacity)
            {
                throw new InvalidOperationException("Nu exista suficiente locuri disponibile pentru marire.");
            }

            sale.Seats += extraSeats;
            _ticketSaleRepository.Update(sale);
        }

        // Java: public int getSoldSeatsForSpectacle(Long spectacleId)
        public int GetSoldSeatsForSpectacle(long spectacleId)
        {
            return _ticketSaleRepository.GetSoldSeatsForSpectacle(spectacleId);
        }
    }
}