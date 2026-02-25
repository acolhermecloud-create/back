using Domain;

namespace API.Payloads
{
    public class CampaignPayload
    {
        public string CategoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal FinancialGoal { get; set; }
        public int Deadline { get; set; }
        public List<string>? Media { get; set; }
        public CampaignStatus? Status { get; set; } = CampaignStatus.Active;
        public CampaignisForWho CampaignisForWho { get; set; } = CampaignisForWho.Me;
        public IFormFile[]? Files { get; set; }

        public string? BeneficiaryName { get; set; }
        public string? Name { get; set; }
        public string? DocumentId { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
