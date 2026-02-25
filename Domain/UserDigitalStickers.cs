using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Domain
{
    public class UserDigitalStickers(
        Guid? campaignId, 
        Guid userId,
        string transactionId,
        int quantity,
        int value,
        Gateway gateway,
        TransationMethod transationMethod,
        DonationStatus donationStatus,
        DonationType type)
    {
        // Propriedade estática para o nome da coleção no MongoDB
        public static string TABLE_NAME => "user_digital_stickers";

        [BsonId]
        [BsonRepresentation(BsonType.String)] // Garante que o Guid seja armazenado como String no MongoDB
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um Id único

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; } = userId;

        [BsonRepresentation(BsonType.String)]
        public Guid? CampaignId { get; set; } = campaignId;

        public string TransactionId { get; set; } = transactionId;
        public int Quantity { get; set; } = quantity;
        public long Value { get; set; } = value;
        public TransationMethod TransactionMethod { get; set; } = transationMethod;
        public DonationStatus Status { get; set; } = donationStatus;
        public DonationType Type { get; set; } = type;
        public Gateway Gateway { get; set; } = gateway;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
