namespace Domain.Interfaces.Repository
{
    public interface IUserDigitalStickersUsageRepository
    {
        Task Add(UserDigitalStickersUsage userDigitalStickersUsage);
        Task<List<UserDigitalStickersUsage>> GetByUserId(Guid userId);
        Task<List<UserDigitalStickersUsage>> ListByCampaignId(Guid campaignId);
    }
}
