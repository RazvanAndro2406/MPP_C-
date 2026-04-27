using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository;

public interface IArtistSpectacleRepository : IRepository<long, ArtistSpectacle>
{
    IEnumerable<ArtistSpectacle> FindByArtistId(long artistId);
    IEnumerable<ArtistSpectacle> FindBySpectacleId(long spectacleId);
    bool ExistsRelation(long artistId, long spectacleId);
}

