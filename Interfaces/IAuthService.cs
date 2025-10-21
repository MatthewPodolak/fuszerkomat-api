using fuszerkomat_api.Data;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;

namespace fuszerkomat_api.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthTokenVMO>> RegisterAsync(RegisterVM model, CancellationToken ct = default);
        Task<Result<AuthTokenVMO>> LoginAsync(LoginVM model, CancellationToken ct = default);
        Task<Result<AuthTokenVMO>> RefreshAsync(string refreshToken, CancellationToken ct = default);
        Task<Result> LogoutAsync(CancellationToken ct = default);
    }
}
