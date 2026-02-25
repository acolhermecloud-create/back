using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class Category(string name, string description)
    {
        public static readonly string TABLE_NAME = "categories"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um Id único

        public string Name { get; set; } = name;

        public string Description { get; set; } = description;
    }
}