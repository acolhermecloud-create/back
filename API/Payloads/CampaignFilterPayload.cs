using Domain;

namespace API.Payloads
{
    public class CampaignFilterPayload
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? Guids { get; set; } = [];
        public string? CategoryId { get; set; }
        public string? Name { get; set; }
        public CampaignStatus? Status { get; set; }
        public bool Listing { get; set; } = true;
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
