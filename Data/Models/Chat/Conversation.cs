using MongoDB.Bson;

namespace fuszerkomat_api.Data.Models.Chat
{
    public class Conversation
    {
        public ObjectId Id { get; set; }
        public int TaskId { get; set; }
        public string OwnerUserId { get; set; } = default!;
        public string CompanyUserId { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
    }
}
