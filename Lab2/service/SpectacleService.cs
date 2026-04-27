using Lab2.domain;
using Lab2.repository;
using org.example.repository;

namespace Lab2.service
{
    public class SpectacleService
    {
        private readonly ISpectacleRepository _repository;
        private readonly BuyerService? _buyerService;

        public SpectacleService(ISpectacleRepository repository, BuyerService? buyerService = null)
        {
            _repository = repository;
            _buyerService = buyerService;
        }

        public List<Spectacle> GetAllSpectacles()
        {
            return _repository.FindAll().ToList();
        }

        public void AddSpectacle(string name, DateTime date, int duration, int capacity, string location)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Capacitatea trebuie sa fie pozitiva!", nameof(capacity));
            }

            var spectacle = new Spectacle(name, date, duration, capacity, location);
            _repository.Save(spectacle);
        }

        public void UpdateSpectacle(Spectacle spectacle)
        {
            var result = _repository.Update(spectacle);
            if (result != null)
            {
                Console.WriteLine($"Update esuat: Spectacolul cu ID {spectacle.Id} nu exista.");
            }
            else
            {
                Console.WriteLine("Update realizat cu succes!");
            }
        }

        public void DeleteSpectacle(long id)
        {
            var deleted = _repository.Delete(id);
            if (!deleted)
            {
                Console.WriteLine($"Delete esuat: Spectacolul cu ID {id} nu exista.");
            }
            else
            {
                Console.WriteLine("Delete realizat cu succes!");
            }
        }

        public Spectacle? GetById(long id)
        {
            return _repository.FindOne(id);
        }

        public int GetAvailableSeats(long spectacleId)
        {
            if (_repository is not ISpectacleRepository spectacleRepository)
            {
                throw new InvalidOperationException("Repository-ul de spectacole nu suporta calculul locurilor disponibile.");
            }

            return spectacleRepository.GetAvailableSeats(spectacleId);
        }

        public void SellTickets(long spectacleId, long buyerId, string buyerName, int numberOfSeats)
        {
            if (string.IsNullOrWhiteSpace(buyerName))
            {
                throw new ArgumentException("Numele cumparatorului nu poate fi gol.", nameof(buyerName));
            }

            if (numberOfSeats <= 0)
            {
                throw new ArgumentException("Numarul de locuri trebuie sa fie pozitiv.", nameof(numberOfSeats));
            }

            if (_repository is not ISpectacleRepository spectacleRepository)
            {
                throw new InvalidOperationException("Repository-ul de spectacole nu suporta vanzarea de bilete.");
            }

            if (_buyerService == null)
            {
                throw new InvalidOperationException("Serviciul de cumparatori nu este disponibil.");
            }

            var buyer = _buyerService.GetOrCreateBuyer(buyerId, buyerName);
            var sold = spectacleRepository.SellTickets(spectacleId, buyer.Id, buyer.Name, numberOfSeats);
            if (!sold)
            {
                throw new InvalidOperationException("Nu exista suficiente locuri disponibile pentru acest spectacol.");
            }
        }

        public List<TicketSale> GetTicketSalesByBuyer(long buyerId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("ID cumparator invalid.", nameof(buyerId));
            }

            return _repository.GetTicketSalesByBuyer(buyerId).ToList();
        }

        public void IncreaseTicketSeats(long ticketSaleId, int extraSeats)
        {
            if (ticketSaleId <= 0)
            {
                throw new ArgumentException("ID bilet invalid.", nameof(ticketSaleId));
            }

            if (extraSeats <= 0)
            {
                throw new ArgumentException("Numarul suplimentar de locuri trebuie sa fie pozitiv.", nameof(extraSeats));
            }

            var updated = _repository.IncreaseTicketSeats(ticketSaleId, extraSeats);
            if (!updated)
            {
                throw new InvalidOperationException("Nu se pot adauga locurile: bilet inexistent sau locuri insuficiente.");
            }
        }
    }
}

