using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
using fuszerkomat_api.VMO;

namespace fuszerkomat_api.Interfaces
{
    public interface ITokenService
    {
        Task<Result<AuthTokenVMO>> CreateTokensAsync(AppUser user, string ip, string? userAgent, CancellationToken ct = default);
        Task<Result<AuthTokenVMO>> RefreshAsync(string rawRefreshToken, string ip, string? userAgent, CancellationToken ct = default);
        Task RevokeAllForUserAsync(string userId, string ip, CancellationToken ct = default);
    }
}
