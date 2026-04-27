using System;
using System.Collections.Generic;
using System.Linq;
using Ticketing.Model.Domain;
using Ticketing.Persistence.Repository;
using Ticketing.Services;

namespace Ticketing.Service
{
    public class SpectacleServiceImpl : ISpectacleService
    {
        private readonly ISpectacleRepository _repository;

        public SpectacleServiceImpl(ISpectacleRepository repository)
        {
            _repository = repository;
        }

        // Java: public List<Spectacle> getAllSpectacles()
        public IList<Spectacle> GetAllSpectacles()
        {
            // Cast and conversion to ToList() to satisfy IList return type
            return _repository.FindAll().ToList();
        }

        // Java: public void addSpectacle(String name, LocalDateTime date, int duration, int capacity, String location)
        public void AddSpectacle(string name, DateTime date, int duration, int capacity, string location)
        {
            if (capacity <= 0) 
                throw new ArgumentException("Capacitatea trebuie sa fie pozitiva!");

            // 0 passed as ID for new entity, matching our base Entity<long> structure
            Spectacle spectacle = new Spectacle(0, name, date, duration, capacity, location);
            _repository.Save(spectacle);
        }

        // Java: public void updateSpectacle(Spectacle s)
        public void UpdateSpectacle(Spectacle s)
        {
            // Standard C# Repository Pattern: returns null if the entity doesn't exist to update
            Spectacle? res = _repository.Update(s);
            
            if (res == null)
            {
                Console.WriteLine($"Update esuat: Spectacolul cu ID {s.Id} nu exista.");
            }
            else
            {
                Console.WriteLine("Update realizat cu succes!");
            }
        }

        // Java: public void deleteSpectacle(Long id)
        public void DeleteSpectacle(long id)
        {
            bool res = _repository.Delete(id);
            if (res)
            {
                Console.WriteLine("Delete realizat cu succes!");
            }
            else
            {
                Console.WriteLine($"Delete esuat: Spectacolul cu ID {id} nu exista.");
            }
        }

        // Java: public Optional<Spectacle> getSpectacleById(Long id)
        public Spectacle? GetSpectacleById(long id)
        {
            return _repository.FindOne(id);
        }
    }
}