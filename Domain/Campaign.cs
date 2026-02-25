using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class Campaign(
        Guid creatorId, Guid categoryId, Guid planId,
        string title, string description, string slug, decimal financialGoal, string beneficiaryName, CampaignisForWho campaignisFor,
        DateTime deadline, List<string> media,
        Guid currentPlanId,
        decimal currentPercentToBeCharged, decimal fixedRate)
    {
        public static readonly string TABLE_NAME = "campaigns"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um Id único

        [BsonRepresentation(BsonType.String)]
        public Guid CreatorId { get; set; } = creatorId; // ID do usuário criador da campanha

        [BsonRepresentation(BsonType.String)]
        public Guid CategoryId { get; set; } = categoryId; // ID da categoria da vaquinha

        [BsonRepresentation(BsonType.String)]
        public Guid PlanId { get; set; } = planId; // ID da categoria da vaquinha

        public string Title { get; set; } = title; // Título da vaquinha
        public string Description { get; set; } = description; // Descrição detalhada da vaquinha
        public decimal FinancialGoal { get; set; } = financialGoal; // Meta financeira
        public DateTime Deadline { get; set; } = deadline; // Prazo de término
        public List<string> Media { get; set; } = media; // Imagens ou vídeos (array de strings para URLs)
        public bool IsApproved { get; set; } = true; // Inicialmente aprovada
        public CampaignStatus Status { get; set; } = CampaignStatus.Active; // Status inicial
        public CampaignisForWho CampaignisForWho { get; set; } = campaignisFor;
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Data de criação
        public DateTime UpdateAt { get; set; } // Data da atualização

        public bool CanReceiveDonation { get; set; } = true;

        public string Slug { get; set; } = slug;
        public string BeneficiaryName { get; set; } = beneficiaryName;

        public string? Reason { get; set; }

        // SE ESSE CAMPO FOR DEFINIDO, ENTÃO DEVE-SE CONSIDERAR ELE AO INVES DO PLANO
        [BsonRepresentation(BsonType.String)]
        public Guid? CurrentPlanId { get; set; } = currentPlanId;
        public decimal? CurrentPercentToBeCharged { get; set; } = currentPercentToBeCharged;
        public decimal? FixedRate { get; set; } = fixedRate;

        public bool? Listing { get; set; } = true;

        public Category? Category { get; set; } = null;

        public List<Donation> Donations { get; set; } = new();

        [BsonIgnore]
        public List<UserDigitalStickersUsage>? DigitalStickers { get; set; } = null;

        [BsonIgnore]
        public LeverageRequest? LeverageRequest { get; set; } = null;

        [BsonIgnore]
        public List<CampaignComments>? Comments { get; set; } = null;

        public User? Creator { get; set; }
    }
}