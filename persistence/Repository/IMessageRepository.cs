using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository
{
    public interface IMessageRepository
    {
        void Save(Message message);
    }
}