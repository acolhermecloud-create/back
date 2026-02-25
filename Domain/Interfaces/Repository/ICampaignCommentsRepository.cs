namespace Domain.Interfaces.Repository
{
    public interface ICampaignCommentsRepository
    {
        Task Add(CampaignComments campaignComment);
        Task<List<CampaignComments>> ListByCampaignId(Guid campaignId);
        Task RemoveById(Guid commentId);
    }
}
