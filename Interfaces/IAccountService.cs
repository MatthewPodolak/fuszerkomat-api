using fuszerkomat_api.Data;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;

namespace fuszerkomat_api.Interfaces
{
    public interface IAccountService
    {
        Task<Result<CompanyProfileVMO>> GetCompanyProfileAsync(string id, CancellationToken ct);
        Task<Result> UpdateCompanyInfrormation(string userId, CompanyProfileInfoVM model, CancellationToken ct);
        Task<Result> UpdateUserInformation(string userId, UserProfileInfoVM model, CancellationToken ct);
        Task<Result> DeleteAccount(string userId, CancellationToken ct);
    }
}
