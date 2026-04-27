using Lab2.domain;
using org.example.repository;

namespace Lab2.repository;

public class IUserRepository:IRepository<long, User>
{
    public User FindOne(long id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<User> FindAll()
    {
        throw new NotImplementedException();
    }

    public User Save(User entity)
    {
        throw new NotImplementedException();
    }

    public bool Delete(long id)
    {
        throw new NotImplementedException();
    }

    public User Update(User entity)
    {
        throw new NotImplementedException();
    }
}