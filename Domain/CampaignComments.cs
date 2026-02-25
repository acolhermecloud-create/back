using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class CampaignComments(Guid campaignId, Guid userId, string comment, DateTime date)
    {
        public static readonly string TABLE_NAME = "campaign_comments";

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public Guid CampaignId { get; set; } = campaignId;

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; } = userId;

        public string Comment { get; set; } = comment;
        public DateTime CreateAt { get; set; } = date;

        [BsonIgnore]
        public string UserName { get; set; }
    }
}
