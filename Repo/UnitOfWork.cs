using fuszerkomat_api.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace fuszerkomat_api.Repo
{
    public class UnitOfWork : IUnitOfWork
    {
        public AppDbContext _context { get; }
        public UnitOfWork(AppDbContext context) => _context = context;

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        {
            return await _context.Database.BeginTransactionAsync(ct);
        }

        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync(ct);
                try
                {
                    await action(ct);
                    await _context.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
           return await _context.SaveChangesAsync(ct);
        }
    }
}
