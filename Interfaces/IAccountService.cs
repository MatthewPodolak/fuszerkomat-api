using fuszerkomat_api.Data;
using fuszerkomat_api.VM;

namespace fuszerkomat_api.Interfaces
{
    public interface IAccountService
    {
        Task<Result> UpdateCompanyInfrormation(string userId, CompanyProfileInfoVM model, CancellationToken ct);
        Task<Result> UpdateUserInformation(string userId, UserProfileInfoVM model, CancellationToken ct);
    }
}
