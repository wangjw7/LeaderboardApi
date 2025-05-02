using LeaderboardApi.Models;
using LeaderboardApi.Models.Repository;
using LeaderboardApi.Interfaces;
namespace LeaderboardApi.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly RequestQueue<Entry> _addUpdateQueue;
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    private bool _isProcessing;

    public LeaderboardService()
    {
        _addUpdateQueue = new RequestQueue<Entry>();
    }

    public void EnqueueEntry(Entry entry)
    {
        _addUpdateQueue.EnterQueue(entry);
        if (!_isProcessing)
        {
            _isProcessing = true;
            Task.Run(ProcessUpdateQueueAsync);
        }
    }

    public async Task<decimal?> ProcessUpdateQueueAsync()
    {
        while (_addUpdateQueue.Count() > 0)
        {
            var entry = await _addUpdateQueue.DeQueueAsync(CancellationToken.None);
            _lock.EnterWriteLock();
            try
            {
                var result = LeaderboardRepository.Update(entry);
                return result;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        _isProcessing = false;
        return null;
    }

    public List<Entry> GetCustomerByRank(int start, int end)
    {
        return LeaderboardRepository.GetCustomerByRank(start, end);
    }
    public List<Entry> GetCustomerByCustomerID(Int64 customerid, int high = 0, int low = 0)
    {
        return LeaderboardRepository.GetCustomerByCustomerID(customerid, high, low);
    }
}