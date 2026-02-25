
using Domain.Interfaces.Services;

namespace API.Jobs
{
    public class MigrationJob(IServiceProvider serviceProvider) : IHostedService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            using var scope = _serviceProvider.CreateScope();
            var _migration = scope.ServiceProvider.GetRequiredService<IMigrationService>();

            await _migration.GroupAccessMigration();
            await _migration.CategoryMigration();
            await _migration.GatewayConfiguration();
            await _migration.DigitalStickerMigration();
            await _migration.PlansMigration();
            await _migration.CreateSystemBankAccount();
            await _migration.RenamePropertieInCampaign();
            await _migration.CreateBAASConfiguration();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
