using Domain.Objects;

namespace Domain.Interfaces.Services
{
    public interface ICampaignService
    {
        Task<Campaign> Create(
            string title, string description, decimal goal, string beneficiaryName, CampaignisForWho forWho,
            DateTime dueDate, Guid categoryId, Guid userId,
            Dictionary<Stream, string> FilesAndExtensions);

        Task<TransactionData> GeneratePaymentData(DonationRequest request);

        Task MakeSmallDonation(
            Guid campaignId, 
            Guid donorId,
            DonationType type,
            int amount);

        Task ConfirmDonation(string transactionId);

        Task<bool> CheckDonation(string transactionId);

        Task<List<Campaign>> GetCampaignsByDonor(Guid donorId, int page, int pageSize);

        Task<List<Donation>> GetDonationsByCampaign(Guid campaignId, int page, int pageSize);

        Task<Donation> GetExtractDonationById(Guid donationId); // Serve para puxar a doação completa (dados de pagamento e etc)

        Task<Campaign> Update(Guid id, string title, string description, decimal goal, DateTime dueDate, Guid categoryId);

        Task UpdateListingCampaign(Guid id, bool listing);

        Task UpdateStatus(Guid id, CampaignStatus status, string? reason);

        Task<List<string>> AddImages(Guid id, Dictionary<Stream, string> NewFilesAndExtensions);

        Task RemoveImages(Guid id, List<string> imagesKeyToRemove);

        Task Delete(Guid id);

        Task<Campaign> GetById(Guid id, int page, int pageSize);

        Task<Campaign> GetBySlug(string slug);

        Task<List<Campaign>> GetByUserId(Guid userId, int page, int pageSize);

        Task<PagedResult<Campaign>> GetFilteredCampaigns(DateTime? startDate, 
            DateTime? endDate, Guid? categoryId, string? name, 
            List<Guid>? campaignIds,
            bool? listing,
            CampaignStatus status, int page, int pageSize);

        Task<PagedResult<Campaign>> GetFilteredCampaigns(DateTime? startDate, DateTime? endDate, string? name, CampaignStatus? status, bool? listing, int page, int pageSize);

        Task<List<Donation>> GetDonations(Guid campaignId, int page, int pageSize);

        Task Report(Guid campaignId,
            Guid? denunciatorId,
            string iAm, 
            string aRespectFor,
            string why,
            string description);

        Task RemoveReportById(Guid reportId);

        Task UpdateReportById(
            Guid reportId,
            string iAm,
            string aRespectFor,
            string why,
            string description);

        Task<List<Complaint>> GetAllReports(int page, int pageSize);

        Task<List<Category>> GetAllCategory();

        Task<List<CampaignLogs>> GetLogs(Guid campaignId);

        Task<BalanceExtractResume> GetUserBalanceAwaitingRelease(Guid userId);

        Task<BalanceExtractResume> GetUserBalanceReleasedForWithdraw(Guid userId);

        Task<List<Plan>> ListPlans();

        Task<PagedResult<Plan>> ListPlans(int page, int pageSize);

        Task AddComment(Guid campaignId, Guid userId, string message);

        Task RemoveComment(Guid commentId);

        // Jobs
        Task CheckCampaignWithoutDonationsInTenDays();

        Task CheckCampaignWithoutDonationsInTwentyDays();

        Task CloseInactiveCampaigns();

        Task RenamePropertie();

        Task RecordUtm(Utm utm);
    }
}