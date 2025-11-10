using fuszerkomat_api.Data;
using fuszerkomat_api.VMO;

namespace fuszerkomat_api.Interfaces
{
    public interface IChatService
    {
        Task<Result<List<ChatVMO>>> GetChatsAsync(string userId, CancellationToken ct);
        Task<Result> ArchiveConversation(string conversationId, CancellationToken ct);
    }
}
