using System.Numerics;

namespace API.Payloads
{
    public class LeveragePayload
    {
        public IFormFile[]? Files { get; set; }
        public string CampaignId { get; set; }
        public string PlanId { get; set; }
        public string Phone { get; set; }

        // Respostas do formulário
        public bool HasSharedCampaign { get; set; }
        public string? EvidenceLinks { get; set; } = string.Empty;
        public bool WantsToBoostCampaign { get; set; }
        public string PreferredContactMethod { get; set; }
    }
}
