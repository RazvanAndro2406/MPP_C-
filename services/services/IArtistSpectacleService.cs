using System.Collections.Generic;
using Ticketing.Model.Domain;

namespace Ticketing.Services
{
    public interface IArtistSpectacleService
    {
        // Java: List<ArtistSpectacle> getAllArtistSpectacles();
        IList<ArtistSpectacle> GetAllArtistSpectacles();

        // Java: void addArtistSpectacle(Long artistId, Long spectacleId);
        void AddArtistSpectacle(long artistId, long spectacleId);

        // Java: void deleteArtistSpectacle(Long id);
        void DeleteArtistSpectacle(long id);

        // Java: Optional<ArtistSpectacle> getArtistSpectacleById(Long id);
        ArtistSpectacle? GetArtistSpectacleById(long id);

        // Java: List<ArtistSpectacle> getArtistSpectacleByArtistId(Long artistId);
        IList<ArtistSpectacle> GetArtistSpectacleByArtistId(long artistId);

        // Java: List<ArtistSpectacle> getArtistSpectacleBySpectacleId(Long spectacleId);
        IList<ArtistSpectacle> GetArtistSpectacleBySpectacleId(long spectacleId);

        // Java: void deleteByArtistAndSpectacle(Long artistId, Long spectacleId);
        void DeleteByArtistAndSpectacle(long artistId, long spectacleId);

        // Java: boolean existsRelationBetweenArtistAndSpectacle(Long artistId, Long spectacleId);
        bool ExistsRelationBetweenArtistAndSpectacle(long artistId, long spectacleId);
    }
}