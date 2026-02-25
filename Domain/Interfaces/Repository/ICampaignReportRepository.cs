namespace Domain.Interfaces.Repository
{
    public interface ICampaignReportRepository
    {
        Task Add(CampaignReport campaignReport);
        Task<List<CampaignReport>> GetByCampaignIdAsync(Guid campaignId);
    }
}
