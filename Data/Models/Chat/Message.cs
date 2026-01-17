using MongoDB.Bson;

namespace fuszerkomat_api.Data.Models.Chat
{
    public class Message
    {
        public ObjectId Id { get; set; }
        public ObjectId ConversationId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string EncryptedPayload { get; set; } = string.Empty;
        public string KeyForRecipient { get; set; } = string.Empty;
        public string KeyForSender { get; set; } = string.Empty;
        public string Iv { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}