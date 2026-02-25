namespace API.Payloads
{
    public class AddMockPayload
    {
        public string CampaignSlug { get; set; }
        public string Password { get; set; }
        public long Goal { get; set; }
        public bool AllowDonations { get; set; }
    }
}
