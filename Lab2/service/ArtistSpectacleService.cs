using Lab2.domain;
using Lab2.repository;

namespace Lab2.service
{
    public class ArtistSpectacleService
    {
        private readonly IArtistSpectacleRepository _repository;

        public ArtistSpectacleService(IArtistSpectacleRepository repository)
        {
            _repository = repository;
        }

        public void AddRelation(long artistId, long spectacleId)
        {
            if (artistId <= 0 || spectacleId <= 0)
            {
                throw new ArgumentException("Artist ID si Spectacle ID trebuie sa fie pozitive.");
            }

            if (_repository.ExistsRelation(artistId, spectacleId))
            {
                throw new ArgumentException("Relatie existenta intre artist si spectacol!");
            }

            var relation = new ArtistSpectacle(artistId, spectacleId);
            _repository.Save(relation);
        }

        public List<ArtistSpectacle> GetAllRelations()
        {
            return _repository.FindAll().ToList();
        }

        public List<ArtistSpectacle> GetByArtist(long artistId)
        {
            return _repository.FindByArtist(artistId).ToList();
        }

        public List<ArtistSpectacle> GetBySpectacle(long spectacleId)
        {
            return _repository.FindBySpectacle(spectacleId).ToList();
        }

        public void DeleteRelation(long id)
        {
            var deleted = _repository.Delete(id);
            if (!deleted)
            {
                Console.WriteLine($"Stergere esuata: Relatie cu ID {id} nu exista.");
            }
            else
            {
                Console.WriteLine("Relatie stearsa cu succes!");
            }
        }

        public void DeleteRelationByArtistAndSpectacle(long artistId, long spectacleId)
        {
            var relations = _repository.FindByArtist(artistId);
            foreach (var relation in relations)
            {
                if (relation.SpectacleId == spectacleId && relation.Id.HasValue)
                {
                    _repository.Delete(relation.Id.Value);
                    return;
                }
            }
        }

        public ArtistSpectacle? GetById(long id)
        {
            return _repository.FindOne(id);
        }

        public bool ExistsRelation(long artistId, long spectacleId)
        {
            return _repository.ExistsRelation(artistId, spectacleId);
        }
    }
}

