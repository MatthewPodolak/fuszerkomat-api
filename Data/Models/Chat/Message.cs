using MongoDB.Bson;

namespace fuszerkomat_api.Data.Models.Chat
{
    public class Message
    {
        public ObjectId Id { get; set; }
        public ObjectId ConversationId { get; set; }
        public string SenderId { get; set; } = default!;
        public string Text { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
