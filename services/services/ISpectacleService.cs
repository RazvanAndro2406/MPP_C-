using System;
using System.Collections.Generic;
using Ticketing.Model.Domain;

namespace Ticketing.Services
{
    public interface ISpectacleService
    {
        // Java: List<Spectacle> getAllSpectacles();
        IList<Spectacle> GetAllSpectacles();

        // Java: void addSpectacle(String name, LocalDateTime date, int duration, int capacity, String location);
        void AddSpectacle(string name, DateTime date, int duration, int capacity, string location);

        // Java: void updateSpectacle(Spectacle spectacle);
        void UpdateSpectacle(Spectacle spectacle);

        // Java: void deleteSpectacle(Long id);
        void DeleteSpectacle(long id);

        // Java: Optional<Spectacle> getSpectacleById(Long id);
        Spectacle? GetSpectacleById(long id);
    }
}