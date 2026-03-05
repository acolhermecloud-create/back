using Domain.Interfaces.Services;

namespace API.Jobs
{
    public class CacheWorker(
        IServiceProvider serviceProvider,
        IBackgroundTaskQueue queue) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IBackgroundTaskQueue _queue = queue;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _queue.DequeueAsync(stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                await workItem(stoppingToken);
            }
        }
    }
}
