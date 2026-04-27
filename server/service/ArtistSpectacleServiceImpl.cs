using System;
using System.Collections.Generic;
using System.Linq;
using Ticketing.Model.Domain;
using Ticketing.Persistence.Repository;
using Ticketing.Services;

namespace Ticketing.Service
{
    public class ArtistSpectacleServiceImpl : IArtistSpectacleService
    {
        private readonly IArtistSpectacleRepository _repository;

        public ArtistSpectacleServiceImpl(IArtistSpectacleRepository repository)
        {
            _repository = repository;
        }

        // Java: public void addArtistSpectacle(Long artistId, Long spectacleId)
        public void AddArtistSpectacle(long artistId, long spectacleId)
        {
            // In C#, long is a value type and cannot be null. 
            // If you used long? (nullable), you would keep the null check.
            
            if (_repository.ExistsRelation(artistId, spectacleId))
            {
                throw new ArgumentException("Relatie existenta intre artist si spectacol!");
            }

            // Creating the relation entity. 0 is passed for ID as it's typically auto-generated.
            ArtistSpectacle relation = new ArtistSpectacle(0, artistId, spectacleId);
            _repository.Save(relation);
        }

        // Java: public List<ArtistSpectacle> getAllArtistSpectacles()
        public IList<ArtistSpectacle> GetAllArtistSpectacles()
        {
            // Cast to List to match the IList return type
            return _repository.FindAll().ToList();
        }

        // Java: public List<ArtistSpectacle> getArtistSpectacleByArtistId(Long artistId)
        public IList<ArtistSpectacle> GetArtistSpectacleByArtistId(long artistId)
        {
            // Calling .ToList() converts IEnumerable to a concrete List
            return _repository.FindByArtistId(artistId).ToList();
        }

        public IList<ArtistSpectacle> GetArtistSpectacleBySpectacleId(long spectacleId)
        {
            return _repository.FindBySpectacleId(spectacleId).ToList();
        }

        // Java: public void deleteArtistSpectacle(Long id)
        public void DeleteArtistSpectacle(long id)
        {
            bool deleted = _repository.Delete(id);
            if (!deleted)
            {
                Console.WriteLine($"Stergere esuat: Relatie cu ID {id} nu exista.");
            }
            else
            {
                Console.WriteLine("Relatie stearsa cu succes!");
            }
        }

        // Java: public void deleteByArtistAndSpectacle(Long artistId, Long spectacleId)
        public void DeleteByArtistAndSpectacle(long artistId, long spectacleId)
        {
            var relations = _repository.FindByArtistId(artistId);
            foreach (var relation in relations)
            {
                // .Equals() in Java for Longs is == in C# for longs
                if (relation.SpectacleId == spectacleId)
                {
                    _repository.Delete(relation.Id);
                    return;
                }
            }
        }

        // Java: public Optional<ArtistSpectacle> getArtistSpectacleById(Long id)
        public ArtistSpectacle? GetArtistSpectacleById(long id)
        {
            return _repository.FindOne(id);
        }

        // Java: public boolean existsRelationBetweenArtistAndSpectacle(Long artistId, Long spectacleId)
        public bool ExistsRelationBetweenArtistAndSpectacle(long artistId, long spectacleId)
        {
            return _repository.ExistsRelation(artistId, spectacleId);
        }
    }
}