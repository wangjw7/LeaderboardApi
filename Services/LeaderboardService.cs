using LeaderboardApi.Models;
using LeaderboardApi.Models.Repository;
using LeaderboardApi.Interfaces;
namespace LeaderboardApi.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly RequestQueue<Entry> _addUpdateQueue;
    private readonly ReaderWriterLockSlim _lock = new();

    public LeaderboardService()
    {
        _addUpdateQueue = new RequestQueue<Entry>();
    }

    public async Task<decimal?> EnqueueEntry(Entry entry)
    {
        _addUpdateQueue.EnterQueue(entry);
        var result = await Task.Run(ProcessUpdateQueueAsync);
        return result;
    }

    private async Task<decimal?> ProcessUpdateQueueAsync()
    {
        while (_addUpdateQueue.Count() > 0)
        {
            var entry = await _addUpdateQueue.DeQueueAsync(CancellationToken.None)
                ?? throw new ProblemException("DeQueue return null entry.", "Unexpected null return");

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
        return null;
    }

    public List<Entry>? GetAll()
    {
        return LeaderboardRepository.GetAll();
    }

    public Entry? GetEntryById(Int64 customerid)
    { 
        return LeaderboardRepository.GetEntryByID(customerid);
    }

    public List<Entry>? GetCustomerByRank(int start, int end)
    {
        return LeaderboardRepository.GetCustomerByRank(start, end);
    }

    public List<Entry>? GetCustomerByCustomerID(Int64 customerid, int high = 0, int low = 0)
    {
        return LeaderboardRepository.GetCustomerByCustomerID(customerid, high, low);
    }
}