using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Domain
{
    public class CampaignLogs(Guid? campaignId, string description, CampaignLogType type)
    {
        public static readonly string TABLE_NAME = "campaign_logs"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public Guid? CampaignId { get; set; } = campaignId;
        public string Description { get; set; } = description;
        public CampaignLogType CampaignLogType { get; set; } = type;
        public DateTime CreateAt { get; set; } = DateTime.Now;
    }
}
