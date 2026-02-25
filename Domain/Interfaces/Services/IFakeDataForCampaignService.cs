namespace Domain.Interfaces.Services
{
    public interface IFakeDataForCampaignService
    {
        Task CreateForCampaign(string slug, bool allowDonations, long? goal = null);
    }
}
