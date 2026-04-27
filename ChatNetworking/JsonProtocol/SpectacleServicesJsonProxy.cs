
using Ticketing.Model.Domain;
using Ticketing.Services;

namespace ChatNetworking.JsonProtocol
{
    public class SpectacleServicesJsonProxy : ISpectacleService
    {
        private readonly INetworkProtocol _delegate;

        public SpectacleServicesJsonProxy(INetworkProtocol networkDelegate)
        {
            _delegate = networkDelegate;
        }

        // Java: public List<Spectacle> getAllSpectacles()
        public IList<Spectacle> GetAllSpectacles()
        {
            return _delegate.GetAllSpectaclesRemote();
        }

        // Java: public void addSpectacle(String name, LocalDateTime date, int duration, int capacity, String location)
        public void AddSpectacle(string name, DateTime date, int duration, int capacity, string location)
        {
            _delegate.AddSpectacleRemote(name, date, duration, capacity, location);
        }

        // Java: public void updateSpectacle(Spectacle spectacle)
        public void UpdateSpectacle(Spectacle spectacle)
        {
            _delegate.UpdateSpectacleRemote(spectacle);
        }

        // Java: public void deleteSpectacle(Long id)
        public void DeleteSpectacle(long id)
        {
            _delegate.DeleteSpectacleRemote(id);
        }

        // Java: public Optional<Spectacle> getSpectacleById(Long id)
        public Spectacle? GetSpectacleById(long id)
        {
            return _delegate.GetSpectacleByIdRemote(id);
        }
    }
}