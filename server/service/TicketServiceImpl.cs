using System;
using System.Collections.Generic;
using System.Linq;
using Ticketing.Model.Domain;
using Ticketing.Persistence.Repository;
using Ticketing.Services;

namespace Ticketing.Service
{
    public class TicketServiceImpl : ITicketService
    {
        private readonly ITicketRepository _repository;

        public TicketServiceImpl(ITicketRepository repository)
        {
            _repository = repository;
        }

        // Java: public List<Ticket> getAllTickets()
        public IList<Ticket> GetAllTickets()
        {
            // Converting IEnumerable from repo to IList via LINQ ToList()
            return _repository.FindAll().ToList();
        }

        // Java: public void addTicket(double price, Long spectacleId)
        public void AddTicket(double price, long spectacleId)
        {
            if (price <= 0) 
                throw new ArgumentException("Pretul trebuie sa fie pozitiv!");
            
            // Note: long in C# is a value type; it cannot be null like Java's Long.
            // If the ID was 0 (invalid), you could check for that here.

            // 0 passed for ID as it's a new entity
            Ticket ticket = new Ticket(0, price, spectacleId);
            _repository.Save(ticket);
        }

        // Java: public void updateTicket(Ticket t)
        public void UpdateTicket(Ticket t)
        {
            // Standard C# Repository convention: returns null if entity not found
            Ticket? res = _repository.Update(t);
            
            if (res == null)
            {
                Console.WriteLine($"Update esuat: Biletul cu ID {t.Id} nu exista.");
            }
            else
            {
                Console.WriteLine("Update realizat cu succes!");
            }
        }

        // Java: public void deleteTicket(Long id)
        public void DeleteTicket(long id)
        {
            bool res = _repository.Delete(id);
            if (!res)
            {
                Console.WriteLine($"Delete esuat: Biletul cu ID {id} nu exista.");
            }
            else
            {
                Console.WriteLine("Delete realizat cu succes!");
            }
        }

        // Java: public Optional<Ticket> getTicketById(Long id)
        public Ticket? GetTicketById(long id)
        {
            return _repository.FindOne(id);
        }

        // Java: public List<Ticket> getTicketsBySpectacleId(Long spectacleId)
        public IList<Ticket> GetTicketsBySpectacleId(long spectacleId)
        {
            // C# LINQ equivalent of Java Stream filter
            return GetAllTickets()
                .Where(t => t.SpectacleId == spectacleId)
                .ToList();
        }
    }
}