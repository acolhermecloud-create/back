using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class UserNotificationsTokens(Guid userId, string token)
    {
        public static readonly string TABLE_NAME = "user_notifications_tokens"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; } = userId;

        public string Token { get; set; } = token;
        public DateTime CreateAt { get; set; } = DateTime.Now;
    }
}
