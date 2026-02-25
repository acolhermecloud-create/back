using Domain.Objects;

namespace Domain.Interfaces.Services
{
    public interface ILeverageRequestService
    {
        Task Record(Guid campaignId, Guid userId, Guid planId, string phone, bool hasSharedCampaign, string evidenceLinks,
            bool wantsToBoostCampaign, string preferredContactMethod, Dictionary<Stream, string> FilesAndExtensions);

        Task ChangeStatus(Guid id, LeverageStatus leverageStatus);

        Task Delete(Guid id);
        Task<PagedResult<LeverageRequest>> GetAll(int page, int size);
        Task<List<LeverageRequest>> GetByUserId(Guid userId);
        Task<LeverageRequest> GetByCampaignId(Guid campaignId);
    }
}
