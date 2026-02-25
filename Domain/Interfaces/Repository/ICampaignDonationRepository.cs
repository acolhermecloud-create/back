namespace Domain.Interfaces.Repository
{
    public interface ICampaignDonationRepository
    {
        Task Add(Donation donation);
        Task<Donation> GetByTransactionId(string transactionId);
        Task<Donation> GetById(Guid id);
        Task<List<Donation>> GetByCampaignId(Guid campaignId);
        Task<List<Donation>> GetByCampaignId(Guid campaignId, int page, int pageSize);
        Task<List<Donation>> GetByCampaignId(Guid campaignId, DonationBalanceStatus donationBalanceStatus);
        Task<List<Donation>> GetByCampaignId(Guid campaignId, DonationStatus donationBalanceStatus);
        Task<List<Donation>> GetByDonorId(Guid donorId, int page, int pageSize);
        Task<List<Donation>> GetAll(int page, int pageSize);
        Task<decimal> GetConversionRate();

        Task<List<Donation>> GetByBalanceStatus(DonationBalanceStatus donationBalanceStatus);

        Task Update(Donation donation);
        Task UpdateMany(List<Donation> donations);
        Task Delete(Guid id);
    }
}
