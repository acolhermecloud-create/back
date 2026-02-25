namespace API.Payloads
{
    public class DonationIntentPayload
    {
        public Guid CampaignId { get; set; }
        public int Value { get; set; }

        public string DonorName { get; set; }
        public string DonorEmail { get; set; }
        public string DonorDocumentId { get; set; }
        public string DonorPhone { get; set; }
        
    }
}
