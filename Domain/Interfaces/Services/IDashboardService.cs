namespace Domain.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<long> TotalRevenue(DateTime startDate, DateTime endDate);

        Task<long> TotalRevenueCustomers(DateTime startDate, DateTime endDate);

        Task<long> FeeCollection(DateTime startDate, DateTime endDate);

        Task<long> TotalDigitalStickers(DateTime startDate, DateTime endDate);

        Task<long> NetProfit(DateTime startDate, DateTime endDate);

        Task<long> TotalOfCustomers();

        Task<long> TotalOfCampaigns();

        Task<long> TotalActiveCampaigns();

        Task<decimal> ConversionRate();
    }
}
