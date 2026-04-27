using Org.Example.Domain;
using org.example.repository;

namespace Lab2.repository;

public interface IArtistRepository:IRepository<long, Artist>
{
    
}