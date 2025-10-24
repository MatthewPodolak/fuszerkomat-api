using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Data.Models.Chat;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.Repo;
using fuszerkomat_api.VMO;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace fuszerkomat_api.Services
{
    public class ChatService : IChatService
    {
        private readonly IRepository<AppUser> _userRepo;
        private readonly ChatCollections _chat;
        private readonly ILogger<IChatService> _logger;
        private readonly IHttpContextAccessor _http;
        public ChatService(IRepository<AppUser> userRepo, ChatCollections chat, ILogger<IChatService> logger, IHttpContextAccessor http)
        {
            _userRepo = userRepo;
            _chat = chat;
            _logger = logger;
            _http = http;
        }
        public async Task<Result<List<ChatVMO>>> GetChatsAsync(string userId, CancellationToken ct)
        {
            try
            {
                var user = await _userRepo.Query().AsNoTracking().Select(u => new { u.Id, u.AccountType }).FirstOrDefaultAsync(a => a.Id == userId, ct);
                if(user == null)
                {
                    _logger.LogWarning("GetChatsAsync tried to acess non existing user. Id={Id} Path={Path}, Method={Method}", userId, _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                    return Result<List<ChatVMO>>.NotFound(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                var f = Builders<Conversation>.Filter;
                FilterDefinition<Conversation> filter = user.AccountType switch
                {
                    AccountType.User => f.Eq(c => c.OwnerUserId, userId),
                    AccountType.Company => f.Eq(c => c.CompanyUserId, userId)
                };

                var convos = await _chat.Conversations
                   .Find(filter)
                   .Sort(Builders<Conversation>.Sort.Descending(c => c.LastMessageAt))
                   .Project(c => new
                   {
                       Id = c.Id,
                       c.OwnerUserId,
                       c.CompanyUserId
                   })
                   .ToListAsync(ct);

                if (convos.Count == 0)
                {
                    return Result<List<ChatVMO>>.Ok(data: new List<ChatVMO>(), traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                var counterpartIds = convos
                    .Select(c => c.OwnerUserId == userId ? c.CompanyUserId : c.OwnerUserId)
                    .Distinct()
                    .ToList();

                List<(string Id, string Name, string Img)> counterpartData;

                if (user.AccountType == AccountType.User)
                {
                    counterpartData = await _userRepo.Query()
                        .AsNoTracking()
                        .Where(u => counterpartIds.Contains(u.Id) && u.AccountType == AccountType.Company)
                        .Include(u => u.CompanyProfile)
                        .Select(u => new ValueTuple<string, string, string>(
                            u.Id,
                            u.CompanyProfile!.CompanyName,
                            u.CompanyProfile!.Img))
                        .ToListAsync(ct);
                }
                else
                {
                    counterpartData = await _userRepo.Query()
                        .AsNoTracking()
                        .Where(u => counterpartIds.Contains(u.Id) && u.AccountType == AccountType.User)
                        .Include(u => u.UserProfile)
                        .Select(u => new ValueTuple<string, string, string>(
                            u.Id,
                            u.UserProfile!.Name,
                            u.UserProfile!.Img))
                        .ToListAsync(ct);
                }


                var counterpartDict = counterpartData.ToDictionary(x => x.Id, x => x);

                var items = convos.Select(c =>
                {
                    var otherId = c.OwnerUserId == userId ? c.CompanyUserId : c.OwnerUserId;
                    counterpartDict.TryGetValue(otherId, out var other);

                    return new ChatVMO
                    {
                        ConversationId = c.Id.ToString(),
                        CorespondentId = otherId,
                        CorespondentName = other.Name ?? "Unknown",
                        CorespondentImg = other.Img ?? string.Empty
                    };
                }).ToList();

                return Result<List<ChatVMO>>.Ok(data: items, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "GetChatsAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<List<ChatVMO>>.Canceled(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in GetChatsAsync. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<List<ChatVMO>>.Internal(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }
    }
}
