using LeaderboardApi.Models;
namespace LeaderboardApi.Interfaces;

public interface ILeaderboardService
{
    Task<decimal?> EnqueueEntry(Entry entry);
    List<Entry> GetCustomerByRank(int start, int end);
    List<Entry> GetCustomerByCustomerID(Int64 customerid, int high = 0, int low = 0);
    List<Entry>? GetAll();
}