using Ticketing.Model.Domain;
using Ticketing.Services;

namespace ChatNetworking.JsonProtocol
{
    public class ArtistServicesJsonProxy : IArtistService
    {
        private readonly INetworkProtocol _delegate;

        public ArtistServicesJsonProxy(INetworkProtocol @delegate)
        {
            _delegate = @delegate;
        }

        // Matches Java: @Override List<Artist> getAllArtists()
        public IList<Artist> GetAllArtists()
        {
            // Delegates to the remote networking logic
            return _delegate.GetAllArtistsRemote();
        }

        // Matches Java: @Override void addArtist(String name)
        public void AddArtist(string name)
        {
            _delegate.AddArtistRemote(name);
        }

        // Matches Java: @Override void updateArtist(Artist artist)
        public void UpdateArtist(Artist artist)
        {
            _delegate.UpdateArtistRemote(artist);
        }

        // Matches Java: @Override void deleteArtist(Long id)
        public void DeleteArtist(long id)
        {
            _delegate.DeleteArtistRemote(id);
        }

        // Matches Java: @Override Optional<Artist> getArtistById(Long id)
        public Artist? GetArtistById(long id)
        {
            return _delegate.GetArtistByIdRemote(id);
        }
    }
}