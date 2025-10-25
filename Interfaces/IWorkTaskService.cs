using fuszerkomat_api.Data;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;

namespace fuszerkomat_api.Interfaces
{
    public interface IWorkTaskService
    {
        Task<Result> PublishAsync(PublishWorkTaskVM model, string userId, CancellationToken ct);
        Task<Result<List<WorkTaskPreviewVMO>>> GetWorkTasksAsync(WorkTaskFilterVM filter, CancellationToken ct);
        Task<Result<UserWorkTaskVMO>> GetWorkTaskForUserAsync(int id, string userId, CancellationToken ct);
        Task<Result<CompanyWorkTaskVMO>> GetWorkTaskForCompanyAsync(int id, string userId, CancellationToken ct);
        Task<Result<ApplyVMO>> ApplyForWorkTaskAsync(ApplyToWorkTaskVM model, string companyId, CancellationToken ct);
        Task<Result<List<UserWorkTaskPreviewVMO>>> GetOwnAsync(OwnWorkTasksFilterVM filters, string userId, CancellationToken ct);
        Task<Result> ChangeApplicationStatusAsync(ChangeApplicationStatusVM model, string userId, CancellationToken ct);
        Task<Result> CompleteRealization(CompleteRealizationVM model, string userId, CancellationToken ct);
    }
}
