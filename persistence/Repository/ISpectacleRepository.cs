using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository
{
    public interface ISpectacleRepository : IRepository<long, Spectacle>
    {
    }
}