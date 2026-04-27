using System.Data;
using Lab2.domain;
using org.example.repository;

namespace Lab2.repository
{
    public interface IArtistSpectacleRepository:IRepository<long, ArtistSpectacle>
    {
        /*ArtistSpectacle? FindOne(long id);
        IEnumerable<ArtistSpectacle> FindAll();
        ArtistSpectacle Save(ArtistSpectacle entity);
        bool Delete(long id);*/
        /*ArtistSpectacle? Update(ArtistSpectacle entity);*/
        IEnumerable<ArtistSpectacle> FindByArtist(long artistId);
        IEnumerable<ArtistSpectacle> FindBySpectacle(long spectacleId);
        bool ExistsRelation(long artistId, long spectacleId);

    }
}

