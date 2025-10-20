using fuszerkomat_api.Data;
using Microsoft.EntityFrameworkCore;

namespace fuszerkomat_api.Repo
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _set;
        public Repository(AppDbContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
        {
            return await _set.FindAsync(new[] { id }, ct);
        }

        public IQueryable<T> Query()
        {
            return _set.AsQueryable();
        }

        public void Delete(T entity)
        {
            _set.Remove(entity);
        }

        public void Add(T entity)
        {
            _set.Add(entity);
        }

        public void Update(T entity)
        {
            _set.Update(entity);
        }

        public async Task AddAsync(T entity, CancellationToken ct = default)
        {
            await _set.AddAsync(entity, ct);
        }

        public async Task AddRangeAsync(ICollection<T> entities, CancellationToken ct = default)
        {
            await _set.AddRangeAsync(entities, ct);
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }
    }
}
