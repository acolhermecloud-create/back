using Domain.Interfaces.Services;
using System.Collections.Concurrent;

namespace Service
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new();
        private readonly SemaphoreSlim _signal = new(0);

        public void Enqueue(Func<CancellationToken, Task> workItem)
        {
            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken ct)
        {
            await _signal.WaitAsync(ct);
            _workItems.TryDequeue(out var workItem);
            return workItem!;
        }
    }
}
