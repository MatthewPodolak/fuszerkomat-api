using fuszerkomat_api.Data;
using fuszerkomat_api.VM;

namespace fuszerkomat_api.Interfaces
{
    public interface IWorkTaskService
    {
        Task<Result> PublishAsync(PublishWorkTaskVM model, string userId, CancellationToken ct);
    }
}
