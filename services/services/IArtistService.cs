using System.Collections.Generic;
using Ticketing.Model.Domain;

namespace Ticketing.Services
{
    public interface IArtistService
    {
        // Java: List<Artist> getAllArtists();
        IList<Artist> GetAllArtists();

        // Java: void addArtist(String name);
        void AddArtist(string name);

        // Java: void updateArtist(Artist artist);
        void UpdateArtist(Artist artist);

        // Java: void deleteArtist(Long id);
        void DeleteArtist(long id);

        // Java: Optional<Artist> getArtistById(Long id);
        Artist? GetArtistById(long id);
    }
}