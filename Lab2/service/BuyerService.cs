using Lab2.domain;
using Lab2.repository;

namespace Lab2.service
{
    public class BuyerService
    {
        private readonly IBuyerRepository _repository;

        public BuyerService(IBuyerRepository repository)
        {
            _repository = repository;
        }

        public Buyer GetOrCreateBuyer(long buyerId, string buyerName)
        {
            if (string.IsNullOrWhiteSpace(buyerName))
            {
                throw new ArgumentException("Numele cumparatorului nu poate fi gol.", nameof(buyerName));
            }

            var normalizedName = buyerName.Trim();

            if (buyerId > 0)
            {
                var buyerById = _repository.FindOne(buyerId);
                if (buyerById != null)
                {
                    if (!buyerById.Name.Equals(normalizedName, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("ID-ul cumparatorului nu corespunde numelui introdus.");
                    }

                    return buyerById;
                }
            }

            var buyers = _repository.FindAll().ToList();
            var existingBuyer = buyers.FirstOrDefault(b => b.Name.Equals(normalizedName, StringComparison.OrdinalIgnoreCase));

            if (existingBuyer != null)
            {
                if (buyerId > 0 && existingBuyer.Id != buyerId)
                {
                    throw new InvalidOperationException($"Numele exista deja cu alt ID: {existingBuyer.Id}.");
                }

                return existingBuyer;
            }

            var newBuyer = new Buyer(normalizedName);
            return _repository.Save(newBuyer);
        }

        public Buyer? GetById(long id)
        {
            return _repository.FindOne(id);
        }

        public List<Buyer> GetAllBuyers()
        {
            return _repository.FindAll().ToList();
        }

        public void DeleteBuyer(long id)
        {
            _repository.Delete(id);
        }
    }
}

