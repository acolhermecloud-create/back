using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Domain
{
    public class CampaignTransactions
        (Guid campaignId, Guid donorId, string transactionId,
        List<TransactionStatus> transactionStatuses, 
        decimal value,
        TransationMethod transationMethod,
        string transactionName)
    {
        public static readonly string TABLE_NAME = "campaigns_transactions"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um Id único

        public Guid CampaignId { get; set; } = campaignId;
        public Guid DonorId { get; set; } = donorId;
        public string TransactionId { get; set; } = transactionId;
        public List<TransactionStatus> TransactionStatus { get; set; } = transactionStatuses;
        public decimal Value { get; set; } = value;
        public TransationMethod TransationMethod { get; set; } = transationMethod;
        public string TransactionName { get; set; } = transactionName;
    }

    public class TransactionStatus()
    {
        public TransactionType Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
