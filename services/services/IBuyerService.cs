using System.Collections.Generic;
using Ticketing.Model.Domain;

namespace Ticketing.Services
{
    public interface IBuyerService
    {
        // Java: Buyer getOrCreateBuyer(String email, String name);
        Buyer GetOrCreateBuyer(string email, string name);

        // Java: Optional<Buyer> getBuyerById(Long id);
        Buyer? GetBuyerById(long id);

        // Java: Optional<Buyer> getBuyerByEmail(String email);
        Buyer? GetBuyerByEmail(string email);

        // Java: List<Buyer> getAllBuyers();
        IList<Buyer> GetAllBuyers();

        // Java: void deleteBuyer(Long id);
        void DeleteBuyer(long id);
    }
}