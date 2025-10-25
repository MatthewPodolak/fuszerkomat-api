using fuszerkomat_api.Data;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;

namespace fuszerkomat_api.Interfaces
{
    public interface IOpinionService
    {
        Task<Result<List<CompanyToRatePreviewVMO>>> GetAll(OpinionFiltersVM filters, string userId, CancellationToken ct);
    }
}
