using Domain;

namespace API.Payloads
{
    public class UpdateCampaignStatusPayload
    {
        public string Id { get; set; }
        public string? Reason { get; set; }
        public CampaignStatus Status { get; set; }
    }
}
