using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.Repo;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using System.Security.Claims;

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
            try
            {
                var user = await _userMgr.FindByEmailAsync(model.Email);
                if (user is null)
                {
                    return Result<AuthTokenVMO>.BadRequest(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty, errors: new() { Error.NotFound(msg: "USER does not exists.") });
                }               

                var pwd = await _signInMgr.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
                if (!pwd.Succeeded)
                {
                    return Result<AuthTokenVMO>.BadRequest(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty, errors: new() { Error.Unauthorized() });
                }
                    
                var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var ua = _http.HttpContext?.Request.Headers.UserAgent.ToString();

                return await _tokens.CreateTokensAsync(user, ip, ua, ct);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "LoginAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<AuthTokenVMO>.Canceled(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginAsync unexcpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<AuthTokenVMO>.Internal(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }

        public async Task<Result> LogoutAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? _http.HttpContext?.User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Result.Unauthorized(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);

                var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                await _tokens.RevokeAllForUserAsync(userId, ip, ct);

                return Result.Ok(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "LogoutAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Canceled(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LogoutAsync unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Internal(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }

        public async Task<Result<AuthTokenVMO>> RefreshAsync(string refreshToken, CancellationToken ct = default)
        {
            try
            {
                var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var ua = _http.HttpContext?.Request.Headers.UserAgent.ToString();

                var res = await _tokens.RefreshAsync(refreshToken, ip, ua, ct);
                return res;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "RefreshAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<AuthTokenVMO>.Canceled(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshAsync unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<AuthTokenVMO>.BadRequest(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty, errors: new() { Error.Internal() });
            }
        }

        public async Task<Result<AuthTokenVMO>> RegisterAsync(RegisterVM model, CancellationToken ct = default)
        {
            try
            {
                var exists = await _userMgr.FindByEmailAsync(model.Email);
                if(exists != null)
                {
                    _logger.LogInformation("User with given email already exists. Email={Email}, Path={Path}, Method={Method}", model.Email, _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                    return Result<AuthTokenVMO>.Conflict(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
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
                            Name = model.Name ?? "User" + new Random().Next(1,9999),
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
                        _logger.LogCritical("Forbidden accounttype acess point. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                        return Result<AuthTokenVMO>.Internal(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
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
            catch(OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "RegisterAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<AuthTokenVMO>.Canceled(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterAsync unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<AuthTokenVMO>.BadRequest(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty, errors: new() { Error.Internal() });
            }
        }
    }
}
