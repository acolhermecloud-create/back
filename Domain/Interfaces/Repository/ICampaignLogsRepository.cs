namespace Domain.Interfaces.Repository
{
    public interface ICampaignLogsRepository
    {
        Task Add(CampaignLogs campaignLog);
        Task<List<CampaignLogs>> GetByCampaignId(Guid campaignId);
    }
}
