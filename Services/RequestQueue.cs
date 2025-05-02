using LeaderboardApi.Models;
using System.Collections.Concurrent;

namespace LeaderboardApi.Services;

public class RequestQueue<Entry>
{
    private readonly ConcurrentQueue<Entry> _queue = new ConcurrentQueue<Entry>();
    private readonly SemaphoreSlim _semaphore= new SemaphoreSlim(0);

    public void EnterQueue(Entry entry)
    {
        _queue.Enqueue(entry);
        _semaphore.Release();
    }

    public async Task<Entry?> DeQueueAsync(CancellationToken token)
    {
        await _semaphore.WaitAsync(token);
        _queue.TryDequeue(out var entry);
        return entry;
    }
    public int Count()
    { 
        return _queue.Count();
    }
}