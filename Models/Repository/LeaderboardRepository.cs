using System.Collections.Concurrent;
namespace LeaderboardApi.Models.Repository;

public static class LeaderboardRepository
{
    private static ConcurrentDictionary<Int64, Entry>? _leaderboard;
    private static List<Entry>? _leaderboardList;

    public static Entry? GetEntryByID(Int64 Key)
    {
        if (_leaderboard != null)
        {
            return _leaderboard[Key];
        }
        else
        {
            return null;
        }
    }

    public static List<Entry>? GetAll()
    {
        if (_leaderboard != null)
        {
            return [.._leaderboard.Values];
        }
        else
        {
            return null;
        }
    }

    public static decimal Update(Entry newEntry)
    {
        // note(wangjw): using dictionary for Leaderboard data save
        // score-descending order sorting.
        if (_leaderboard == null)
        {
            _leaderboard = new ConcurrentDictionary<Int64, Entry>();
            newEntry.Rank = 1;

            // TODO(wangjw): hash collision.
            _leaderboard.AddOrUpdate(newEntry.CustomerID, newEntry, (i, NewEntry) => NewEntry);

            // TODO(wangjw): ToList() Performance benchmark.
            _leaderboardList = [.. _leaderboard.Values];

            return newEntry.Score;
        }
        else
        {
            // note(wangjw): every time after update or add entry to the Leaderboard, check this 
            // dictionary is sorted by score and by id for same score.
            if (_leaderboard.TryGetValue(newEntry.CustomerID, out Entry? entry))
            {
                entry.Score += newEntry.Score;
            }
            else
            {
                _leaderboard.AddOrUpdate(newEntry.CustomerID, newEntry, (i, NewEntry) => NewEntry);
            }

            _leaderboardList = [.. _leaderboard.Values.OrderByDescending(x => x.Score).ThenBy(x => x.CustomerID)];
            
            var sc = new ScoreCompare();

            // note(wangjw): search the index of the added or updated entry for rank sorting. 
            int ret;
            decimal result = 0;
            if (entry == null)
            {
                ret = _leaderboardList.BinarySearch(newEntry, sc);
                result = newEntry.Score;
            }
            else
            {
                ret = _leaderboardList.BinarySearch(entry, sc);
                result = entry.Score;
            }

            // note(wangjw): ~0 = -1. update rank.
            if (ret < 0)
            {
                for (var i = ~ret; i < _leaderboardList.Count; i++)
                {
                    _leaderboardList[i].Rank = i + 1;
                }
            }
            else
            {
                for (var i = ret; i < _leaderboardList.Count; i++)
                {
                    _leaderboardList[i].Rank = i + 1;
                }
            }

            return result;
        }
    }

    public static List<Entry>? GetCustomerByRank(int start, int end)
    {
        if (_leaderboard == null)
            throw new ProblemException("No data in LeaderboardList.", "No data");

        var startIndex = start - 1;
        var count = end - start + 1;

        if (count + startIndex > _leaderboardList?.Count)
            throw new ProblemException("Request list length great than Leaderboard data.", "Data access violation");

        return _leaderboardList?.GetRange(startIndex, count);
    }

    public static List<Entry>? GetCustomerByCustomerID(Int64 customerid, int high = 0, int low = 0)
    {
        int rank;

        if (_leaderboard == null)
            return null;

        if(!_leaderboard.TryGetValue(customerid, out Entry? value))
            throw new ProblemException($"No data found with id:{customerid}.", "ID error");

        rank = value.Rank;

        if (rank - low < 0 || rank + high > _leaderboard.Count)
            throw new ProblemException("Request list length great than Leaderboard data.", "Data access violation");

        var result = GetCustomerByRank(rank - low, rank + high);
        return result;
    }
}

public class ScoreCompare : IComparer<Entry>
{
    public int Compare(Entry? x, Entry? y)
    {
        try
        {
            return x.Score.CompareTo(y.Score);
        }
        catch (Exception)
        {

            throw;
        }
    }
}