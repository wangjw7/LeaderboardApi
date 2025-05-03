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
            return _leaderboardList;
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
            var addIsSucceeded = _leaderboard.TryAdd(newEntry.CustomerID, newEntry);
            if (!addIsSucceeded)
                throw new ProblemException("Add data failure.", "Fatal error.");

            // TODO(wangjw): ToList() Performance benchmark.
            _leaderboardList = [.. _leaderboard.Values];
            var result = newEntry.Score;
            return result;
        }
        else
        {
            // note(wangjw): every time after update or add entry to the Leaderboard, check this 
            // dictionary is sorted by score and by id for same score.
            if (_leaderboard.TryGetValue(newEntry.CustomerID, out Entry? entry))
            {
                entry.Score += newEntry.Score;
                if (entry.Score > 1000 || entry.Score < -1000)
                    throw new ProblemException("Score > 1000 or score < -1000 after updated.", "Unexpected score.");
            }
            else
            {
                var addIsSucceeded = _leaderboard.TryAdd(newEntry.CustomerID, newEntry);
                if (!addIsSucceeded)
                    throw new ProblemException("Add data failure.", "Fatal error.");
            }

            _leaderboardList = [.. _leaderboard.Values.OrderByDescending(x => x.Score).ThenBy(x => x.CustomerID)];

            // note(wangjw): search the index of the added or updated entry for rank sorting. 
            int ret;
            decimal result = 0;

            if (entry == null)
            {
                ret = BinarySearchForRepeatElement(_leaderboardList, newEntry);
                result = newEntry.Score;
            }
            else
            {
                ret = BinarySearchForRepeatElement(_leaderboardList, entry);
                result = entry.Score;
            }
            // note(wangjw): ~0 = -1. update rank.
            if (ret < 0)
            {
                throw new ProblemException("Add data failure.", "Fatal error.");
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

        var result = _leaderboardList?.GetRange(startIndex, count);
        return result;
    }

    public static List<Entry>? GetCustomerByCustomerID(Int64 customerid, int high = 0, int low = 0)
    {
        int rank;

        if (_leaderboard == null)
            return null;

        if (!_leaderboard.TryGetValue(customerid, out Entry? value))
            throw new ProblemException($"No data found with id:{customerid}.", "ID valid");

        rank = value.Rank;
        var start = rank - high;
        var end = rank + low;

        if (start < 0 || end > _leaderboard.Count)
            throw new ProblemException("Request list length great than Leaderboard data.", "Data access violation");

        var result = GetCustomerByRank(start, end);
        return result;
    }

    private static int BinarySearchForRepeatElement(List<Entry> source, Entry entry)
    {
        int low = 0, high = source.Count - 1;
        while (low <= high)
        {
            int mid = (low + high) / 2;

            // note(wangjw): if get the same score, assuming the index is on the random location of the same score
            // scale.
            if (source[mid].Score.Equals(entry.Score))
            {
                // note(wangjw): left search
                if (mid > 0)
                {
                    if (source[mid - 1].Score.Equals(entry.Score))
                    {
                        for (int i = mid - 1; i >= 0; i--)
                        {
                            if (source[i].Score.Equals(entry.Score) && source[i].CustomerID == entry.CustomerID)
                            {
                                return i;
                            }
                            else if(!source[i].Score.Equals(entry.Score))
                                break;
                        }
                    }
                }

                if (source[mid].CustomerID == entry.CustomerID)
                {
                    return mid;
                }

                // note(wangjw): right search
                if (mid < high)
                {
                    if (source[mid + 1].Score.Equals(entry.Score))
                    {
                        for (int i = mid + 1; i <= high; i++)
                        {
                            if (source[i].Score.Equals(entry.Score) && source[i].CustomerID == entry.CustomerID)
                            {
                                return i;
                            }
                            else if (!source[i].Score.Equals(entry.Score))
                                throw new ProblemException("Cant find specific data in the list.", "Fatal error");
                        }
                    }
                }
            }

            // note(wangjw): regular binarysearch but descending.
            else if (entry.Score < source[mid].Score)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }
        return -1;
    }
}   