namespace org.example.repository
{
    public interface IRepository<TId, TEntity>
    {
        TEntity FindOne(TId id);
        IEnumerable<TEntity> FindAll();
        TEntity Save(TEntity entity);
        bool Delete(TId id);
        TEntity Update(TEntity entity);
    }
}