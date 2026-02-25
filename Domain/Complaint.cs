using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Domain
{
    public class Complaint(Guid campaignId, Guid? denunciatorId,
        string iAm,
        string aRespectFor,
        string why,
        string description)
    {
        public static readonly string TABLE_NAME = "complaints"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public Guid CampaignId { get; set; } = campaignId;

        [BsonRepresentation(BsonType.String)]
        public Guid? DenunciatorId { get; set; } = denunciatorId;

        public string IAm { get; set; } = iAm;
        public string ARespectFor { get; set; } = aRespectFor;
        public string Why { get; set; } = why;
        public string Description { get; set; } = description;

        [BsonIgnore]
        public Campaign? Campaign { get; set; }
    }
}
