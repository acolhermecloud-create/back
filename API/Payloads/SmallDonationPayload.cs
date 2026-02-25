namespace API.Payloads
{
    public class SmallDonationPayload
    {
        public Guid CampaignId { get; set; }
        public int Quantity { get; set; }
    }
}
