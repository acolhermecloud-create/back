using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class CodeChallenge(string referenceId, string code, DateTime validAt)
    {
        public static readonly string TABLE_NAME = "code_challenge"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um Id único

        public string Reference { get; set; } = referenceId;
        public string Code { get; set; } = code;
        public DateTime ValidAt { get; set; } = validAt;
    }
}
