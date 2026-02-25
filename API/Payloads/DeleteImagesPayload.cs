namespace API.Payloads
{
    public class DeleteImagesPayload
    {
        public string CampaignId { get; set; }
        public string[] ImagesKeys { get; set; }
    }
}
