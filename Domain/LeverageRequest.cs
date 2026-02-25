using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class LeverageRequest(Guid campaignId, Guid userId, Guid planId, string phone, bool hasSharedCampaign,
        string evidenceLinks, bool wantsToBoostCampaign,
        string preferredContactMethod, List<string> files)
    {
        public static readonly string TABLE_NAME = "leverageRequest"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public Guid CampaignId { get; set; } = campaignId;

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; } = userId;

        [BsonRepresentation(BsonType.String)]
        public Guid PlanId { get; set; } = planId;

        public string Phone { get; set; } = phone;

        public LeverageStatus LeverageStatus { get; set; } = LeverageStatus.UnderReview;

        // Respostas do formulário
        public bool HasSharedCampaign { get; set; } = hasSharedCampaign; // Sim ou Não
        public string EvidenceLinks { get; set; } = evidenceLinks; // Links ou descrição de evidências
        public bool WantsToBoostCampaign { get; set; } = wantsToBoostCampaign; // Sim ou Não
        public string PreferredContactMethod { get; set; } = preferredContactMethod; // Ligação, Whatsapp, Email
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public List<string> Files { get; set; } = files;

        [BsonIgnore]
        public List<string> FilesLink { get; set; } = new();

        [BsonIgnore]
        public User? User { get; set; } = null;

        [BsonIgnore]
        public Campaign? Campaign { get; set; }

        [BsonIgnore]
        public Plan? Plan { get; set; }
    }
}
