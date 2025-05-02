using LeaderboardApi.Models;

namespace LeaderboardApi.Interfaces;

public interface ILeaderboardService
{
    void EnqueueEntry(Entry entry);
    public Task<decimal?> ProcessUpdateQueueAsync();
    List<Entry> GetCustomerByRank(int start, int end);

    List<Entry> GetCustomerByCustomerID(Int64 customerid, int high = 0, int low = 0);
}