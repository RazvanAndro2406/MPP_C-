using Lab2.repository;
using Org.Example.Domain;
using org.example.repository;

namespace Lab2.service
{
    public class ArtistService
    {
        private readonly IArtistRepository _repository;

        public ArtistService(IArtistRepository repository)
        {
            _repository = repository;
        }

        public List<Artist> GetAllArtists()
        {
            return _repository.FindAll().ToList();
        }

        public void AddArtist(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Numele artistului nu poate fi gol!", nameof(name));
            }

            var artist = new Artist(name);
            _repository.Save(artist);
        }

        public void UpdateArtist(Artist artist)
        {
            var result = _repository.Update(artist);
            if (result != null)
            {
                Console.WriteLine($"Update esuat: Artistul cu ID {artist.Id} nu exista.");
            }
            else
            {
                Console.WriteLine("Update realizat cu succes!");
            }
        }

        public void DeleteArtist(long id)
        {
            var deleted = _repository.Delete(id);
            if (!deleted)
            {
                Console.WriteLine($"Delete esuat: Artistul cu ID {id} nu exista.");
            }
            else
            {
                Console.WriteLine("Delete realizat cu succes!");
            }
        }

        public Artist? GetById(long id)
        {
            return _repository.FindOne(id);
        }
    }
}

