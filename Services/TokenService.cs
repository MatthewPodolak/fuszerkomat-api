using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Data.Models.Token;
using fuszerkomat_api.Helpers;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.Repo;
using fuszerkomat_api.VMO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace fuszerkomat_api.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _cfg;
        private readonly UserManager<AppUser> _userMgr;
        private readonly IRepository<RefreshToken> _rtRepo;
        private readonly ILogger<ITokenService> _logger;
        private readonly IHttpContextAccessor _http;
        public TokenService(IConfiguration cfg, UserManager<AppUser> userMgr, IRepository<RefreshToken> rtRepo, ILogger<ITokenService> logger, IHttpContextAccessor http)
        {
            _cfg = cfg;
            _userMgr = userMgr;
            _rtRepo = rtRepo;
            _logger = logger;
            _http = http;
        }

        public async Task<Result<AuthTokenVMO>> CreateTokensAsync(AppUser user, string ip, string? userAgent, CancellationToken ct = default)
        {
            try
            {
                var jwt = _cfg.GetSection("AuthSettingsJwt");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var roles = await _userMgr.GetRolesAsync(user);

                var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id),
                new("account_type", user.AccountType.ToString())
            };
                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

                var accessMins = int.Parse(jwt["AccessTokenMinutes"] ?? "15");
                var accessExp = DateTime.UtcNow.AddMinutes(accessMins);

                var token = new JwtSecurityToken(
                    issuer: jwt["Issuer"],
                    audience: jwt["Audience"],
                    claims: claims,
                    expires: accessExp,
                    signingCredentials: creds
                );
                var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

                var rawRefresh = TokenHashing.NewOpaqueTokenBase64();
                var hash = TokenHashing.Hash(rawRefresh, jwt["RefreshTokenHashSecret"]!);
                var refreshDays = int.Parse(jwt["RefreshTokenDays"] ?? "7");
                var refreshExp = DateTime.UtcNow.AddDays(refreshDays);

                await _rtRepo.AddAsync(new RefreshToken
                {
                    TokenHash = hash,
                    UserId = user.Id,
                    ExpiresAtUtc = DateTime.UtcNow.AddDays(refreshDays),
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedByIp = ip,
                    UserAgent = userAgent
                }, ct);

                await _rtRepo.SaveChangesAsync(ct);

                var res = new AuthTokenVMO
                {
                    AcessToken = accessToken,
                    AcessTokenExpires = accessExp,
                    RefreshToken = rawRefresh,
                    RefreshTokenExpires = refreshExp
                };

                return Result<AuthTokenVMO>.Ok(data: res, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch(OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "CreateTokensAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<AuthTokenVMO>.Canceled(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in CreateTokensAsync. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<AuthTokenVMO>.Internal(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }

        public async Task<Result<AuthTokenVMO>> RefreshAsync(string rawRefreshToken, string ip, string? userAgent, CancellationToken ct = default)
        {
            try
            {
                var jwt = _cfg.GetSection("AuthSettingsJwt");
                var hash = TokenHashing.Hash(rawRefreshToken, jwt["RefreshTokenHashSecret"]!);

                var existing = await _rtRepo.Query()
                    .Include(r => r.User)
                    .SingleOrDefaultAsync(r => r.TokenHash == hash, ct);

                if (existing is null)
                {
                    return Result<AuthTokenVMO>.BadRequest(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty, errors: new List<Error> { Error.Unauthorized("Invalid refresh token") });
                }

                if (!existing.IsActive)
                {
                    await RevokeAllForUserAsync(existing.UserId, ip, ct);
                    return Result<AuthTokenVMO>.BadRequest(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty, errors: new List<Error> { Error.Unauthorized("Refresh token reuse detected") });
                }

                existing.RevokedAtUtc = DateTime.UtcNow;
                existing.RevokedByIp = ip;
                existing.LastUsedAtUtc = DateTime.UtcNow;

                var newRaw = TokenHashing.NewOpaqueTokenBase64();
                var newHash = TokenHashing.Hash(newRaw, jwt["RefreshTokenHashSecret"]!);
                existing.ReplacedByTokenHash = newHash;

                var refreshDays = int.Parse(jwt["RefreshTokenDays"] ?? "7");
                var newExp = DateTime.UtcNow.AddDays(refreshDays);

                await _rtRepo.AddAsync(new RefreshToken
                {
                    TokenHash = newHash,
                    UserId = existing.UserId,
                    ExpiresAtUtc = newExp,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedByIp = ip,
                    UserAgent = userAgent
                }, ct);

                await _rtRepo.SaveChangesAsync(ct);

                var created = await CreateTokensAsync(existing.User, ip, userAgent, ct);

                if (created.Success && created.Data is not null)
                {
                    created.Data.RefreshToken = newRaw;
                    created.Data.RefreshTokenExpires = newExp;
                    return created;
                }

                return Result<AuthTokenVMO>.Internal(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "RefreshAsync canceled Path={Path} Method={Method}",
                    _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<AuthTokenVMO>.Canceled(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshAsync error Path={Path} Method={Method}",
                    _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<AuthTokenVMO>.Internal(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }

        public async Task RevokeAllForUserAsync(string userId, string ip, CancellationToken ct = default)
        {
            try
            {
                var active = _rtRepo.Query()
                    .Where(r => r.UserId == userId && r.RevokedAtUtc == null && r.ExpiresAtUtc > DateTime.UtcNow);

                await active.ForEachAsync(r =>
                {
                    r.RevokedAtUtc = DateTime.UtcNow;
                    r.RevokedByIp = ip;
                }, ct);

                await _rtRepo.SaveChangesAsync(ct);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "RevokeAllForUserAsync canceled Path={Path} Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RevokeAllForUserAsync error Path={Path} Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                throw;
            }
        }
    }

}