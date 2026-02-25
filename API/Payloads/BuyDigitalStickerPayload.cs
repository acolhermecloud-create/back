namespace API.Payloads
{
    public class BuyDigitalStickerPayload
    {
        public string? CampaignId { get; set; }
        public string PlanId { get; set; }
        public int Qtd { get; set; }
    }
}
