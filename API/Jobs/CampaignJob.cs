using Domain;
using Domain.Interfaces.Services;
using Hangfire;

namespace API.Jobs
{
    public class CampaignJob(IServiceProvider serviceProvider) : IHostedService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var _campaignService = scope.ServiceProvider.GetRequiredService<ICampaignService>();

            RecurringJob.AddOrUpdate(
                "CheckCampaignWithoutDonationsInTenDays",
                () => _campaignService.CheckCampaignWithoutDonationsInTenDays(),
                "0 8 * * *" // Executa todos os dias às 08:00
            );

            RecurringJob.AddOrUpdate(
                "CheckCampaignWithoutDonationsInTwentyDays",
                () => _campaignService.CheckCampaignWithoutDonationsInTwentyDays(),
                "0 9 * * *" // Executa todos os dias às 09:00
            );

            RecurringJob.AddOrUpdate(
                "CloseInactiveCampaigns",
                () => _campaignService.CloseInactiveCampaigns(),
                "0 10 * * *" // Executa todos os dias às 10:00
            );

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
