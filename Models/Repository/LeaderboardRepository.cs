namespace SortingGame.Models.Repository
{
    public static class LeaderboardRepository
    {
        private static Dictionary<Int64, Entry> Leaderboard;
        private static List<Entry> LeaderboardList;

        public static decimal Update(Entry NewEntry)
        {
            // note(wangjw): using dictionary implement for Leaderboard data
            // score-descending order sorting.
            if (Leaderboard == null)
            {
                Leaderboard = new Dictionary<Int64, Entry>();
                NewEntry.Rank = 1;
                Leaderboard.Add(NewEntry.CustomerID, NewEntry);
                return NewEntry.Score;
            }
            else
            {
                // note(wangjw): every time after update or add entry to the Leaderboard, ensure this 
                // dictionary is sorted by score and by id for same score.
                if (Leaderboard.ContainsKey(NewEntry.CustomerID))
                {
                    Leaderboard[NewEntry.CustomerID].Score = NewEntry.Score;
                }
                else
                {
                    Leaderboard.Add(NewEntry.CustomerID, NewEntry);
                }

                Leaderboard.OrderBy(x => x.Value.Score).ThenBy(x => x.Key);
                LeaderboardList = Leaderboard.Values.ToList();
                // solution 1.
                var cc = new CustomerIdCompare();
                int retUpdate = LeaderboardList.BinarySearch(NewEntry, cc);
                for (int i = retUpdate; i < LeaderboardList.Count(); i++)
                {
                    LeaderboardList[i].Rank = i;
                }
                return NewEntry.Score;
            }
        }

        public static List<Entry>? GetCustomerByRank(int start, int end)
        {
            if (LeaderboardList == null)
                return null;
            if (end - start < 0)
            {
                throw new ProblemException("End rank less than start rank.", "Invalid parameter value");
            }
            if (end - start >= LeaderboardList.Count())
            {
                throw new ProblemException("Request rank length great than Leaderboard data.", "Data access violation");
            }
            return LeaderboardList.GetRange(start - 1, end - start + 1);
        }

        public static List<Entry>? GetCustomerByCustomerID(Int64 customerid, int high = 0, int low = 0)
        {
            var cc = new CustomerIdCompare();
            var entry = new Entry { CustomerID = customerid };
            if (LeaderboardList == null)
                return null;
            int ret = LeaderboardList.BinarySearch(entry, cc);
            var list = LeaderboardList?.GetRange(ret - low, ret + high);
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
}