using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Domain
{
    public class CampaignReport(Guid campaignId, string reason, string explanation)
    {
        public static readonly string TABLE_NAME = "campaign_report"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CampaignId { get; set; } = campaignId;
        public string Reason { get; set; } = reason;
        public string Explanation { get; set; } = explanation;
        public DateTime CreateAt { get; set; } = DateTime.Now;
    }
}
