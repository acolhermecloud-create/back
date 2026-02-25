using Domain.Interfaces.Services;
using Hangfire;

namespace API.Jobs
{
    public class BankJob(IServiceProvider serviceProvider) : IHostedService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var _bankService = scope.ServiceProvider.GetRequiredService<IBankService>();

            RecurringJob.AddOrUpdate(
                "ReleaseBalanceJob",
                () => _bankService.ReleaseBalance(),
                "*/10 * * * *"
            );
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
