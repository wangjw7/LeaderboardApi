using System.Collections.Concurrent;

namespace LeaderboardApi.Models.Repository;

public static class LeaderboardRepository
{
    private static ConcurrentDictionary<Int64, Entry>? _leaderboard;
    private static List<Entry>? _leaderboardList;

    public static Entry GetEntry(Int64 Key)
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

    public static decimal Update(Entry newEntry)
    {
        // note(wangjw): using dictionary implement for Leaderboard data
        // score-descending order sorting.
        if (_leaderboard == null)
        {
            _leaderboard = new ConcurrentDictionary<Int64, Entry>();
            newEntry.Rank = 1;
            _leaderboard.AddOrUpdate(newEntry.CustomerID, newEntry, (i, NewEntry) => NewEntry);

            // TODO(wangjw): ToList() Performance check.
            _leaderboardList = _leaderboard.Values.ToList();

            return newEntry.Score;
        }
        else
        {
            // note(wangjw): every time after update or add entry to the Leaderboard, ensure this 
            // dictionary is sorted by score and by id for same score.
            if (_leaderboard.ContainsKey(newEntry.CustomerID))
            {
                _leaderboard[newEntry.CustomerID].Score = newEntry.Score;
            }
            else
            {
                _leaderboard.AddOrUpdate(newEntry.CustomerID, newEntry, (i, NewEntry) => NewEntry);
            }

            _leaderboard.OrderBy(x => x.Value.Score).ThenBy(x => x.Key);

            // TODO(wangjw): ToList() Performance check.
            _leaderboardList = _leaderboard.Values.ToList();

            // solution 1.
            var cc = new CustomerIdCompare();
            int ret = _leaderboardList.BinarySearch(newEntry, cc);

            //for (int i = ret; i < LeaderboardList.Count(); i++)
            //{
            //    LeaderboardList[i].Rank = i;
            //}
            //return NewEntry.Score;
            // solution 2.
            Parallel.For(ret, _leaderboardList.Count(), i =>
            {
                _leaderboardList[i].Rank = i + 1;
            });
            return newEntry.Score;
        }
    }

    public static List<Entry>? GetCustomerByRank(int start, int end)
    {
        if (_leaderboardList == null)
            throw new ProblemException("No data in LeaderboardList.", "No data");

        if (end - start < 0)

            throw new ProblemException("End rank less than start rank.", "Invalid parameter value");

        if (end - start >= _leaderboardList.Count())

            throw new ProblemException("Request rank length great than Leaderboard data.", "Data access violation");

        return _leaderboardList.GetRange(start - 1, end - start + 1);
    }

    public static List<Entry>? GetCustomerByCustomerID(Int64 customerid, int high = 0, int low = 0)
    {
        var cc = new CustomerIdCompare();
        var entry = new Entry { CustomerID = customerid };
        if (_leaderboardList == null)
            return null;
        int ret = _leaderboardList.BinarySearch(entry, cc);
        var list = _leaderboardList?.GetRange(ret - low, ret + high);
        return list;
    }
}
public class CustomerIdCompare : IComparer<Entry>
{
    public int Compare(Entry? x, Entry? y)
    {
        return x.CustomerID.CompareTo(y.CustomerID);
    }
}
public class ScoreCompare : IComparer<Entry>
{
    public int Compare(Entry? x, Entry? y)
    {
        return x.Score.CompareTo(y.Score);
    }
}
