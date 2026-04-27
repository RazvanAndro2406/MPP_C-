using System.Collections.Generic;
using Ticketing.Model.Domain;

namespace Ticketing.Persistence.Repository
{
    // C# equivalent of your Java UserRepository interface
    public interface IUserRepository : IRepository<long, User>
    {
        // Matches Java: User findBy(String username, String passwd);
        User? FindBy(string username, string password);

        // Matches Java: Iterable<User> getFriendsOf(User user);
        IEnumerable<User> GetFriendsOf(User user);
    }
}