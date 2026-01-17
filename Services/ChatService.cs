using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Data.Models.Chat;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.Repo;
using fuszerkomat_api.VMO;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using static fuszerkomat_api.Helpers.DomainExceptions;

namespace fuszerkomat_api.Services
{
    public class ChatService : IChatService
    {
        private readonly IRepository<AppUser> _userRepo;
        private readonly IRepository<WorkTask> _workTaskRepo;
        private readonly ChatCollections _chat;
        private readonly ILogger<IChatService> _logger;
        private readonly IHttpContextAccessor _http;
        public ChatService(IRepository<AppUser> userRepo, IRepository<WorkTask> workTaskRepo, ChatCollections chat, ILogger<IChatService> logger, IHttpContextAccessor http)
        {
            _userRepo = userRepo;
            _workTaskRepo = workTaskRepo;
            _chat = chat;
            _logger = logger;
            _http = http;
        }
        public async Task<Result<List<ChatVMO>>> GetChatsAsync(string userId, CancellationToken ct)
        {
            var user = await _userRepo.Query().AsNoTracking().Select(u => new { u.Id, u.AccountType }).FirstOrDefaultAsync(a => a.Id == userId, ct);
            if (user == null)
            {
                throw new NotFoundException(logData: new { userId });
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
                   c.CompanyUserId,
                   c.TaskId,
                   c.IsArchive
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

            List<(string Id, string Name, string PublicKey, string PublicSignKey, string Img)> counterpartData;

            if (user.AccountType == AccountType.User)
            {
                counterpartData = await _userRepo.Query()
                    .AsNoTracking()
                    .Where(u => counterpartIds.Contains(u.Id) && u.AccountType == AccountType.Company)
                    .Include(u => u.CompanyProfile)
                    .Select(u => new ValueTuple<string, string, string, string, string>(
                        u.Id,
                        u.CompanyProfile!.CompanyName,
                        u.PublicKey ?? string.Empty,
                        u.PublicSignKey ?? string.Empty,
                        u.CompanyProfile!.Img))
                    .ToListAsync(ct);
            }
            else
            {
                counterpartData = await _userRepo.Query()
                    .AsNoTracking()
                    .Where(u => counterpartIds.Contains(u.Id) && u.AccountType == AccountType.User)
                    .Include(u => u.UserProfile)
                    .Select(u => new ValueTuple<string, string, string, string, string>(
                        u.Id,
                        u.UserProfile!.Name,
                        u.PublicKey ?? string.Empty,
                        u.PublicSignKey ?? string.Empty,
                        u.UserProfile!.Img))
                    .ToListAsync(ct);
            }


            var counterpartDict = counterpartData.ToDictionary(x => x.Id, x => x);
            var convoIds = convos.Select(c => c.Id).ToList();

            var lastMsgs = await _chat.Messages.Aggregate()
                .Match(Builders<Message>.Filter.In(m => m.ConversationId, convoIds))
                .SortByDescending(m => m.CreatedAt)
                .Group(m => m.ConversationId, g => new
                {
                    ConversationId = g.Key,
                    EncryptedPayload = g.First().EncryptedPayload,
                    KeyForRecipient = g.First().KeyForRecipient,
                    KeyForSender = g.First().KeyForSender,
                    Iv = g.First().Iv,
                    SenderId = g.First().SenderId
                })
                .ToListAsync(ct);

            var lastMsgDict = lastMsgs.ToDictionary(
                x => x.ConversationId,
                x => new LastChatMsgVMO
                {
                    EncryptedPayload = x.EncryptedPayload,
                    KeyForRecipient = x.KeyForRecipient,
                    KeyForSender = x.KeyForSender,
                    Iv = x.Iv,
                    Own = x.SenderId == userId
                });

            var taskIds = convos.Select(c => c.TaskId).Where(id => id > 0).Distinct().ToList();
            var taskDict = new Dictionary<int, TaskChatVMO>();
            if (taskIds.Count > 0)
            {
                var tasks = await _workTaskRepo.Query()
                    .AsNoTracking().Include(a => a.Applications)
                    .Where(t => taskIds.Contains(t.Id))
                    .Select(t => new TaskChatVMO
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Desc = t.Desc,
                        CreatorId = t.CreatedByUserId,
                        Status = t.Status,
                        ApplicationStatus = user.AccountType == AccountType.Company ? t.Applications.Where(a => a.CompanyUserId == userId).Select(c => new ApplicationStatusVMO()
                        {
                            Status = c.Status,
                        }).FirstOrDefault() : null,
                    })
                    .ToListAsync(ct);

                taskDict = tasks.ToDictionary(t => t.Id, t => t);
            }

            var items = convos.Select(c =>
            {
                var otherId = c.OwnerUserId == userId ? c.CompanyUserId : c.OwnerUserId;
                var hasOther = counterpartDict.TryGetValue(otherId, out var other);

                lastMsgDict.TryGetValue(c.Id, out var lm);
                taskDict.TryGetValue(c.TaskId, out var td);

                return new ChatVMO
                {
                    ConversationId = c.Id.ToString(),
                    CorespondentId = otherId,
                    CorespondentName = hasOther ? (other.Name ?? "Unknown") : "Unknown",
                    CorespondentImg = hasOther ? (other.Img ?? string.Empty) : string.Empty,
                    CorespondentPublicKey = hasOther ? other.PublicKey : string.Empty,
                    CorespondentPublicSignKey = hasOther ? other.PublicSignKey : string.Empty,
                    IsArchived = c.IsArchive,
                    LastMsg = lm,
                    TaskData = td,
                };
            }).ToList();

            return Result<List<ChatVMO>>.Ok(data: items, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
        }

        public async Task<Result> ArchiveConversation(string convesationId, CancellationToken ct)
        {
            var f = Builders<Conversation>.Filter;
            var filter = f.Eq(c => c.Id, new ObjectId(convesationId));

            var update = Builders<Conversation>.Update
                .Set(c => c.IsArchive, true);

            var result = await _chat.Conversations.UpdateOneAsync(filter, update, cancellationToken: ct);

            if (result.ModifiedCount == 0)
            {
                throw new InternalException();
            }

            return Result.Ok(errors: null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
        }
    }
}
