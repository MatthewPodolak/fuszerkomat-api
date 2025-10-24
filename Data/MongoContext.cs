using fuszerkomat_api.Data.Models.Chat;
using Microsoft.VisualBasic;
using MongoDB.Driver;

namespace fuszerkomat_api.Data
{
    public class MongoContext
    {
        public string ConnectionString { get; set; } = default!;
        public string Database { get; set; } = "fuszerkomat-api";
        public MongoCollectionsOptions Collections { get; set; } = new();
    }

    public sealed class MongoCollectionsOptions
    {
        public string Conversations { get; set; } = "conversations";
        public string Messages { get; set; } = "messages";
    }

    public sealed class ChatCollections
    {
        public IMongoCollection<Conversation> Conversations { get; }
        public IMongoCollection<Message> Messages { get; }

        public ChatCollections(IMongoDatabase db, MongoCollectionsOptions c)
        {
            Conversations = db.GetCollection<Conversation>(c.Conversations);
            Messages = db.GetCollection<Message>(c.Messages);
        }
    }
}
