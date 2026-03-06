
namespace DinExApi.Infra;

internal sealed class Repository<TEntity>(DinExDbContext dbContext) : IRepository<TEntity> where TEntity : class
{
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => dbContext.Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        => dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);

    public void Update(TEntity entity)
        => dbContext.Set<TEntity>().Update(entity);

    public void UpdateRange(IEnumerable<TEntity> entities)
        => dbContext.Set<TEntity>().UpdateRange(entities);

    public void Delete(TEntity entity)
        => dbContext.Set<TEntity>().Remove(entity);

    public void DeleteRange(IEnumerable<TEntity> entities)
        => dbContext.Set<TEntity>().RemoveRange(entities);

    public async Task<IReadOnlyCollection<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>()
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public IQueryable<TEntity> Query(bool asNoTracking = true)
    {
        var query = dbContext.Set<TEntity>().AsQueryable();
        return asNoTracking ? query.AsNoTracking() : query;
    }
}
