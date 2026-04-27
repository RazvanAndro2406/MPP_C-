using Ticketing.Model.Domain;

namespace Ticketing.Services
{
    public interface IChatServices
    {
        // Java: void login(User user, IChatObserver client) throws ChatException;
        void Login(User user, IChatObserver client);

        // Java: void sendMessage(Message message) throws ChatException;
        void SendMessage(Message message);

        // Java: void logout(User user, IChatObserver client) throws ChatException;
        void Logout(User user, IChatObserver client);

        // Java: User[] getLoggedFriends(User user) throws ChatException;
        User[] GetLoggedFriends(User user);

        // Java: void domainDataChanged(String entityType) throws ChatException;
        void DomainDataChanged(string entityType);
    }
}