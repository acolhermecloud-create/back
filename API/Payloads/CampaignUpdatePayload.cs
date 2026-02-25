using Domain;

namespace API.Payloads
{
    public class CampaignUpdatePayload
    {
        public string? Id { get; set; }
        public string? CategoryId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal FinancialGoal { get; set; }
        public int Deadline { get; set; } = 7;
        public List<string>? Media { get; set; }
        public CampaignStatus? Status { get; set; } = CampaignStatus.Active;
    }
}
