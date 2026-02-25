using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class UserDigitalStickersUsage(
        Guid userId,
        Guid campaignId,
        int quantity)
    {
        // Propriedade estática para o nome da coleção no MongoDB
        public static string TABLE_NAME => "user_digital_stickers_usage";

        [BsonId]
        [BsonRepresentation(BsonType.String)] // Garante que o Guid seja armazenado como String no MongoDB
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um Id único

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; } = userId;

        [BsonRepresentation(BsonType.String)]
        public Guid CampaignId { get; set; } = campaignId;

        public int Quantity { get; set; } = quantity;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
