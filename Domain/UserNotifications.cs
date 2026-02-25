using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class UserNotifications
        (Guid userId, string title, string body, string data, bool read)
    {
        public static readonly string TABLE_NAME = "user_notifications"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; } = userId;

        public string Title { get; set; } = title;
        public string Body { get; set; } = body;
        public string Data { get; set; } = data;
        public bool Read { get; set; } = read;
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
