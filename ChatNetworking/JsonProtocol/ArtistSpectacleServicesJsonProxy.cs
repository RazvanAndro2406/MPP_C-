
using Ticketing.Model.Domain;
using Ticketing.Services;

namespace ChatNetworking.JsonProtocol
{
    public class ArtistSpectacleServicesJsonProxy : IArtistSpectacleService
    {
        private readonly INetworkProtocol _delegate;

        public ArtistSpectacleServicesJsonProxy(INetworkProtocol networkDelegate)
        {
            _delegate = networkDelegate;
        }

        // Java: List<ArtistSpectacle> getAllArtistSpectacles()
        public IList<ArtistSpectacle> GetAllArtistSpectacles()
        {
            return _delegate.GetAllArtistSpectaclesRemote();
        }

        // Java: void addArtistSpectacle(Long artistId, Long spectacleId)
        public void AddArtistSpectacle(long artistId, long spectacleId)
        {
            _delegate.AddArtistSpectacleRemote(artistId, spectacleId);
        }

        // Java: void deleteArtistSpectacle(Long id)
        public void DeleteArtistSpectacle(long id)
        {
            _delegate.DeleteArtistSpectacleRemote(id);
        }

        // Java: Optional<ArtistSpectacle> getArtistSpectacleById(Long id)
        public ArtistSpectacle? GetArtistSpectacleById(long id)
        {
            return _delegate.GetArtistSpectacleByIdRemote(id);
        }

        // Java: List<ArtistSpectacle> getArtistSpectacleByArtistId(Long artistId)
        public IList<ArtistSpectacle> GetArtistSpectacleByArtistId(long artistId)
        {
            return _delegate.GetByArtistIdRemote(artistId);
        }

        // Java: List<ArtistSpectacle> getArtistSpectacleBySpectacleId(Long spectacleId)
        public IList<ArtistSpectacle> GetArtistSpectacleBySpectacleId(long spectacleId)
        {
            return _delegate.GetBySpectacleIdRemote(spectacleId);
        }

        // Java: void deleteByArtistAndSpectacle(Long artistId, Long spectacleId)
        public void DeleteByArtistAndSpectacle(long artistId, long spectacleId)
        {
            _delegate.DeleteByArtistAndSpectacleRemote(artistId, spectacleId);
        }

        // Java: boolean existsRelationBetweenArtistAndSpectacle(Long artistId, Long spectacleId)
        public bool ExistsRelationBetweenArtistAndSpectacle(long artistId, long spectacleId)
        {
            return _delegate.ExistsRelationRemote(artistId, spectacleId);
        }
    }
}