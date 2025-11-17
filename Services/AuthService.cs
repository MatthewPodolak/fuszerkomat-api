using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.Repo;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using System.Security.Claims;
using static fuszerkomat_api.Helpers.DomainExceptions;

namespace fuszerkomat_api.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userMgr;
        private readonly SignInManager<AppUser> _signInMgr;
        private readonly ITokenService _tokens;
        private readonly IUnitOfWork _uow;

        private readonly ILogger<IAuthService> _logger;
        private readonly IHttpContextAccessor _http;
        public AuthService(UserManager<AppUser> userMgr, SignInManager<AppUser> signInMgr, ITokenService tokens, IUnitOfWork uow, ILogger<IAuthService> logger, IHttpContextAccessor http)
        {
            _userMgr = userMgr;
            _signInMgr = signInMgr;
            _tokens = tokens;
            _uow = uow;
            _logger = logger;
            _http = http;
        }

        public async Task<Result<AuthTokenVMO>> LoginAsync(LoginVM model, CancellationToken ct = default)
        {
            var user = await _userMgr.FindByEmailAsync(model.Email);
            if (user is null)
            {
                throw new NotFoundException(message: "User with given email does not exists.");
            }

            var pwd = await _signInMgr.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
            if (!pwd.Succeeded)
            {
                throw new UnauthorizedException();
            }

            var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var ua = _http.HttpContext?.Request.Headers.UserAgent.ToString();

            return await _tokens.CreateTokensAsync(user, ip, ua, ct);
        }

        public async Task<Result> LogoutAsync(CancellationToken ct = default)
        {
            var userId = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? _http.HttpContext?.User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedException();

            var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            await _tokens.RevokeAllForUserAsync(userId, ip, ct);

            return Result.Ok(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
        }

        public async Task<Result<AuthTokenVMO>> RefreshAsync(string refreshToken, CancellationToken ct = default)
        {
            var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var ua = _http.HttpContext?.Request.Headers.UserAgent.ToString();

            var res = await _tokens.RefreshAsync(refreshToken, ip, ua, ct);
            return res;
        }

        public async Task<Result<AuthTokenVMO>> RegisterAsync(RegisterVM model, CancellationToken ct = default)
        {
            var exists = await _userMgr.FindByEmailAsync(model.Email);
            if (exists != null)
            {
                throw new ConflictException(message: "User with given email already exists.");
            }

            var user = new AppUser()
            {
                Email = model.Email,
                UserName = model.Name ?? model.CompanyName,
                AccountType = model.AccountType,
                EmailConfirmed = true,
            };

            string role = "User";

            switch (model.AccountType)
            {
                case AccountType.User:
                    var userAcc = new UserProfile()
                    {
                        Email = model.Email,
                        AppUser = user,
                        Name = model.Name ?? "User" + new Random().Next(1, 9999),
                    };
                    user.UserProfile = userAcc;
                    role = "User";
                    break;
                case AccountType.Company:
                    var companyAcc = new CompanyProfile()
                    {
                        AppUser = user,
                        Email = model.Email,
                        CompanyName = model.CompanyName ?? "Company" + new Random().Next(1, 9999),
                        Address = new Address()
                        {
                            Country = "Polska",
                        }
                    };
                    user.CompanyProfile = companyAcc;
                    role = "Company";
                    break;
                default:
                    throw new InternalException();
            }

            Result<AuthTokenVMO>? final = null;

            await _uow.ExecuteInTransactionAsync(async innerCt =>
            {
                var create = await _userMgr.CreateAsync(user, model.Password);
                if (!create.Succeeded)
                    throw new InvalidOperationException(string.Join(" | ", create.Errors.Select(e => e.Description)));

                var addRole = await _userMgr.AddToRoleAsync(user, role);
                if (!addRole.Succeeded)
                    throw new InvalidOperationException(string.Join(" | ", addRole.Errors.Select(e => e.Description)));

                var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var ua = _http.HttpContext?.Request.Headers.UserAgent.ToString();
                final = await _tokens.CreateTokensAsync(user, ip, ua, innerCt);

            }, ct);

            return final!;
        }
    }
}
