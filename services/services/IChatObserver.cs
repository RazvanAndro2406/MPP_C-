using Ticketing.Model.Domain;

namespace Ticketing.Services
{
    public interface IChatObserver
    {
        // Java: void messageReceived(Message message) throws ChatException;
        void MessageReceived(Message message);

        // Java: void friendLoggedIn(User friend) throws ChatException;
        void FriendLoggedIn(User friend);

        // Java: void friendLoggedOut(User friend) throws ChatException;
        void FriendLoggedOut(User friend);

        // Java: void domainDataChanged(String entityType) throws ChatException;
        void DomainDataChanged(string entityType);
    }
}