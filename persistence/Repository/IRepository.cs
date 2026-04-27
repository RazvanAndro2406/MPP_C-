namespace Ticketing.Persistence.Repository;

/// <summary>
/// Generic repository interface matching Lab2 architecture
/// </summary>
public interface IRepository<TId, TEntity> where TEntity : class
{
    TEntity? FindOne(TId id);
    IEnumerable<TEntity> FindAll();
    TEntity Save(TEntity entity);
    bool Delete(TId id);
    TEntity? Update(TEntity entity);
}

