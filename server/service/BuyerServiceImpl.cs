using System;
using System.Collections.Generic;
using System.Linq;
using Ticketing.Model.Domain;
using Ticketing.Persistence.Repository;
using Ticketing.Services;

namespace Ticketing.Service
{
    public class BuyerServiceImpl : IBuyerService
    {
        private readonly IBuyerRepository _repository;

        public BuyerServiceImpl(IBuyerRepository repository)
        {
            _repository = repository;
        }

        // Java: public Buyer getOrCreateBuyer(String email, String name)
        public Buyer GetOrCreateBuyer(string email, string name)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email-ul este obligatoriu.");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Numele este obligatoriu.");
            }

            string normalizedEmail = email.Trim().ToLower();

            // 1. Try to find the buyer by the unique email
            // Mapped Java Optional existing.isPresent() to null check
            Buyer? existing = _repository.FindByEmail(normalizedEmail);

            if (existing != null)
            {
                return existing;
            }

            // 2. If not found, create a new one
            // Assuming ID is 0 for a new entity to be saved
            Buyer newBuyer = new Buyer(0, name.Trim(), normalizedEmail);
            _repository.Save(newBuyer);

            return newBuyer;
        }

        // Java: public Optional<Buyer> getBuyerByEmail(String email)
        public Buyer? GetBuyerByEmail(string email)
        {
            return _repository.FindByEmail(email);
        }

        // Java: public Optional<Buyer> getBuyerById(Long id)
        public Buyer? GetBuyerById(long id)
        {
            return _repository.FindOne(id);
        }

        // Java: public List<Buyer> getAllBuyers()
        public IList<Buyer> GetAllBuyers()
        {
            // Using LINQ ToList() to match the Java logic of creating a new List from an Iterable
            return _repository.FindAll().ToList();
        }

        // Java: public void deleteBuyer(Long id)
        public void DeleteBuyer(long id)
        {
            _repository.Delete(id);
        }
    }
}