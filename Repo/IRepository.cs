namespace fuszerkomat_api.Repo
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> Query();
        Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
        Task AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(ICollection<T> entities, CancellationToken ct = default);
        void Update(T entity);
        void Add(T entity);
        void Delete(T entity);
    }
}
