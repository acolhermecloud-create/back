namespace Domain.Objects
{
    public class DonationRequest
    {
        public Guid CampaignId { get; set; }
        public string DonorEmail { get; set; }
        public string DonorName { get; set; }
        public string DonorDocumentId { get; set; }
        public string DonorPhone { get; set; }
        public TransationMethod TransationMethod { get; set; }
        public DonationType DonationType { get; set; }
        public long Value { get; set; }
        public long Amount { get; set; }
        public DateTime DonateAt { get; set; }
        public string ClientIp { get; set; } = string.Empty;
    }
}
