using fuszerkomat_api.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace fuszerkomat_api.Repo
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
        AppDbContext _context { get; }
    }
}