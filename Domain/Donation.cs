using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class Donation(Guid? campaignId, 
        Guid donorId, string transactionId, 
        DonationType type,
        TransationMethod transactionMethod, 
        int value, 
        int amount,
        DateTime donatedAt,
        DonationStatus donationStatus,
        DonationBalanceStatus balanceStatus,
        Gateway gateway)
    {
        public static readonly string TABLE_NAME = "donations"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public Guid? CampaignId { get; set; } = campaignId;

        [BsonRepresentation(BsonType.String)]
        public Guid DonorId { get; set; } = donorId;

        public string TransactionId { get; set; } = transactionId;

        public TransationMethod TransactionMethod { get; set; } = transactionMethod;

        public DateTime DonatedAt { get; set; } = donatedAt;

        public int Value { get; set; } = value; // IN DECIMAL

        public int Amount { get; set; } = amount;

        public DonationType Type { get; set; } = type;

        public DonationStatus Status { get; set; } = donationStatus;

        public DonationBalanceStatus BalanceStatus { get; set; } = balanceStatus;

        public Gateway Gateway { get; set; } = gateway;

        [BsonIgnore]
        public User? Donor { get; set; }
    }
}
