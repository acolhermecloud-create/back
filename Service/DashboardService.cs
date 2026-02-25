using Domain;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Repository.Bank;
using Domain.Interfaces.Services;

namespace Service
{
    public class DashboardService : IDashboardService
    {
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly IBankTransactionRepository _bankTransactionRepository;

        private readonly IUserRepository _userRepository;
        private readonly ICampaignRepository _campaignRepository;
        private readonly ICampaignDonationRepository _campaignDonationRepository;

        private readonly IUtilityService _utilityService;

        public DashboardService(
            IBankAccountRepository bankAccountRepository,
            IBankTransactionRepository bankTransactionRepository,
            IUserRepository userRepository,
            ICampaignRepository campaignRepository,
            ICampaignDonationRepository campaignDonationRepository,
            IUtilityService utilityService)
        {
            _bankAccountRepository = bankAccountRepository;
            _bankTransactionRepository = bankTransactionRepository;

            _campaignDonationRepository = campaignDonationRepository;

            _userRepository = userRepository;
            _campaignRepository = campaignRepository;
            _utilityService = utilityService;
        }

        public async Task<long> FeeCollection(DateTime startDate, DateTime endDate)
        {
            var transactions = await _bankTransactionRepository.GetTransactions(startDate, endDate, BankTransactionType.CashIn);
            decimal total = Math.Round(transactions.Sum(x => x.Tax), 2); // Arredonda para 2 casas
            long totalInCents = (long)(total * 100m);

            return totalInCents;
        }

        public async Task<long> NetProfit(DateTime startDate, DateTime endDate)
        {
            var transactions = await _bankTransactionRepository.GetTransactions(startDate, endDate, BankTransactionType.CashIn);

            var feeCollections = transactions.Sum(x => x.Tax) * 100;
            var stickers = await TotalDigitalStickers(startDate, endDate);

            return ((long)feeCollections) + stickers;
        }

        public async Task<long> TotalDigitalStickers(DateTime startDate, DateTime endDate)
        {
            var transactions = await _bankTransactionRepository.GetTransactions(startDate, endDate,
                BankTransactionType.CashIn, BankTransactionSource.DigitalSticker);

            decimal total = Math.Round(transactions.Sum(x => x.Liquid), 2); // Arredonda antes de converter
            long totalInCents = (long)(total * 100m);

            return totalInCents;
        }

        public async Task<long> TotalRevenue(DateTime startDate, DateTime endDate)
        {
            var transactions = await _bankTransactionRepository.GetTransactions(startDate, endDate, BankTransactionType.CashIn);
            decimal total = Math.Round(transactions.Sum(x => x.Gross), 2);
            long totalInCents = (long)(total * 100m);

            return totalInCents;
        }

        public async Task<long> TotalRevenueCustomers(DateTime startDate, DateTime endDate)
        {
            var defaultSystemAccount = await _bankAccountRepository.GetSystemAccount();

            var transactions = await _bankTransactionRepository.GetAllTransactionsExceptTheSystemOne(defaultSystemAccount.Id, startDate,
                endDate, BankTransactionType.CashIn);

            decimal total = Math.Round(transactions.Sum(x => x.Liquid), 2);
            long totalInCents = (long)(total * 100m);

            return totalInCents;
        }

        public async Task<long> TotalOfCampaigns()
        {
            return await _campaignRepository.TotalCampaigns();
        }

        public async Task<long> TotalActiveCampaigns()
        {
            return await _campaignRepository.TotalActiveCampaigns();
        }

        public async Task<long> TotalOfCustomers()
        {
            return await _userRepository.TotalUsers(UserType.Common);
        }

        public async Task<decimal> ConversionRate()
        {
            var donations = await _campaignDonationRepository.GetConversionRate();

            return donations;
        }
    }
}
