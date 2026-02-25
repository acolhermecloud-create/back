using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class UserPoints(Guid userId, int point, List<AddedPoints> addedPoints)
    {
        public static readonly string TABLE_NAME = "points"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; } = userId;

        public int CurrentPoints { get; set; } = point;
        public List<AddedPoints> AddedPoints { get; set; } = addedPoints;

    }

    public class AddedPoints
    {
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
