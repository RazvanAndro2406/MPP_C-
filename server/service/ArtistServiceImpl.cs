using System;
using System.Collections.Generic;
using System.Linq;
using Ticketing.Model.Domain;
using Ticketing.Persistence.Repository;
using Ticketing.Services;

namespace Ticketing.Server.Service
{
    public class ArtistServiceImpl : IArtistService
    {
        private readonly IArtistRepository repository;

        public ArtistServiceImpl(IArtistRepository repository)
        {
            this.repository = repository;
        }

        // Java: List<Artist> getAllArtists()
        public IList<Artist> GetAllArtists()
        {
            // We use .ToList() to convert the IEnumerable from the repo to a List
            return repository.FindAll().ToList();
        }

        // Java: void addArtist(String name)
        public void AddArtist(string name)
        {
            // C# equivalent of name == null || name.trim().isEmpty()
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Numele artistului nu poate fi gol!");
            }

            Artist artist = new Artist(name);
            repository.Save(artist);
        }

        // Java: void updateArtist(Artist a)
        public void UpdateArtist(Artist a)
        {
            // In C#, we use null instead of Optional. 
            // Based on your Java logic: res.isPresent() (failure) -> res != null
            Artist? res = repository.Update(a);
            
            if (res != null)
            {
                Console.WriteLine("Update esuat: Artistul cu ID " + a.Id + " nu exista.");
            }
            else
            {
                Console.WriteLine("Update realizat cu succes!");
            }
        }

        // Java: void deleteArtist(Long id)
        public void DeleteArtist(long id)
        {
            bool res = repository.Delete(id);
            if (!res)
            {
                Console.WriteLine("Delete esuat: Artistul cu ID " + id + " nu exista.");
            }
            else
            {
                Console.WriteLine("Delete realizat cu succes!");
            }
        }

        // Java: Optional<Artist> getArtistById(Long id)
        public Artist? GetArtistById(long id)
        {
            return repository.FindOne(id);
        }
    }
}