using Domain.Objects;

namespace Domain.Interfaces.Repository
{
    public interface ICampaignRepository
    {
        // Adiciona uma nova campanha
        Task Add(Campaign campaign);

        // Atualiza uma campanha existente
        Task Update(Campaign campaign);

        // Remove uma campanha pelo ID
        Task Delete(Guid id);

        // Busca uma campanha pelo ID
        Task<Campaign> GetById(Guid id);

        Task<Campaign> GetBySlug(string slug);

        Task<List<Campaign>> GetByCreatorId(Guid creatorId, int page, int pageSize);

        Task<List<Campaign>> GetByCreatorId(Guid creatorId);

        // Retorna todas as campanhas
        Task<PagedResult<Campaign>> GetCampaigns(DateTime? startDate, DateTime? endDate, 
            Guid? categoryId, string? title, 
            List<Guid> ids, CampaignStatus? status,
            bool? listing,
            int page, int pageSize);

        Task<PagedResult<Campaign>> GetCampaigns(DateTime? startDate, DateTime? endDate, string? name, CampaignStatus? status, bool? listing, int page, int pageSize);

        Task<List<Campaign>> GetCampaignsByStatusAndMinAge(CampaignStatus status, DateTime date, int daysBefore);

        Task<bool> ExistSlug(string slug);

        Task RenameNewPlanIdToCurrentPlanIdAsync();

        Task RenameNewPercentToBeChargedToCurrentPercentToBeCharged();

        Task<long> TotalCampaigns();

        Task<long> TotalActiveCampaigns();
    }
}