using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository;

public interface IBuyerRepository : IRepository<long, Buyer>
{
    Buyer? FindByName(string name);
    Buyer? FindByEmail(string email);
}

