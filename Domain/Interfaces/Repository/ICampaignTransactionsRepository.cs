namespace Domain.Interfaces.Repository
{
    public interface ICampaignTransactionsRepository
    {
        Task<CampaignTransactions> GetTransactionByIdAsync(Guid id);
        Task<IEnumerable<CampaignTransactions>> GetTransactionsByCampaignIdAsync(Guid campaignId);
        Task<IEnumerable<CampaignTransactions>> GetTransactionsByDonorIdAsync(Guid donorId);
        Task AddTransactionAsync(CampaignTransactions transaction);
        Task UpdateTransactionAsync(CampaignTransactions transaction);
        Task DeleteTransactionAsync(Guid id);
    }
}
