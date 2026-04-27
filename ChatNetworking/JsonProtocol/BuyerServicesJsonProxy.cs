
using Ticketing.Model.Domain;
using Ticketing.Services;

namespace ChatNetworking.JsonProtocol
{
    public class BuyerServicesJsonProxy : IBuyerService
    {
        private readonly INetworkProtocol _delegate;

        public BuyerServicesJsonProxy(INetworkProtocol networkDelegate)
        {
            _delegate = networkDelegate;
        }

        // Java: public Buyer getOrCreateBuyer(String email, String name)
        public Buyer GetOrCreateBuyer(string email, string name)
        {
            return _delegate.GetOrCreateBuyerRemote(email, name);
        }

        // Java: public Optional<Buyer> getBuyerById(Long id)
        public Buyer? GetBuyerById(long id)
        {
            return _delegate.GetBuyerByIdRemote(id);
        }

        // Java: public Optional<Buyer> getBuyerByEmail(String email)
        public Buyer? GetBuyerByEmail(string email)
        {
            return _delegate.GetBuyerByEmailRemote(email);
        }

        // Java: public List<Buyer> getAllBuyers()
        public IList<Buyer> GetAllBuyers()
        {
            return _delegate.GetAllBuyersRemote();
        }

        // Java: public void deleteBuyer(Long id)
        public void DeleteBuyer(long id)
        {
            _delegate.DeleteBuyerRemote(id);
        }
    }
}