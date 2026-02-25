using Domain.Objects;

namespace Domain.Interfaces.Repository
{
    public interface ILeverageRequestRepository
    {
        Task Record(LeverageRequest leverageRequest);
        Task Update(LeverageRequest leverageRequest);
        Task Delete(Guid id);
        Task<LeverageRequest> GetById(Guid id);
        Task<PagedResult<LeverageRequest>> GetAll(int page, int size);
        Task<List<LeverageRequest>> GetByUserId(Guid userId);
        Task<LeverageRequest> GetByCampaignId(Guid campaignId);
    }
}
