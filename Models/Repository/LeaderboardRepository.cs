namespace SortingGame.Models.Repository
{
    public static class LeaderboardRepository
    {
        private static List<Entry>? Leaderboard;

        public static decimal Update(Entry NewEntry)
        {
            // note(wangjw): using custom IComparer implement for Leaderboard data
            // Binary search and score-descending order sorting.
            if (Leaderboard == null)
            {
                Leaderboard = new List<Entry>();
                NewEntry.Rank = 1;
                Leaderboard.Add(NewEntry);
                return NewEntry.Score;
            }
            else
            {
                var cc = new CustomerIdCompare();
                int ret = Leaderboard.BinarySearch(NewEntry, cc);

                // note(wangjw): every time after update or add entry to the Leaderboard, ensure this 
                // leaderboard is sorted by score and by id for same score.
                if (ret >= 0)
                {
                    Leaderboard[ret].Score = NewEntry.Score;
                }
                else
                {
                    Leaderboard.Add(NewEntry);
                }

                Leaderboard.Sort((y, x) =>
                {
                    int sort = x.Score.CompareTo(y.Score);
                    if (sort == 0)
                        sort = x.CustomerID.CompareTo(y.CustomerID);
                    return sort;
                });

                // solution 1.
                int retUpdate = Leaderboard.BinarySearch(NewEntry, cc);
                for (int i = retUpdate; i < Leaderboard.Count(); i++)
                {
                    Leaderboard[i].Rank = i;
                }
                return NewEntry.Score;
            }
        }

        public static List<Entry>? GetCustomerByRank(int start, int end)
        {
            if (Leaderboard == null)
                return null;
            if (end - start < 0)
            {
                throw new ProblemException("End rank less than start rank.", "Invalid parameter value");
            }
            if (end - start >= Leaderboard.Count())
            {
                throw new ProblemException("Request rank length great than Leaderboard data.", "Data access violation");
            }
            return Leaderboard?.GetRange(start - 1, end - start + 1);
        }

        public static List<Entry>? GetCustomerByCustomerID(Int64 customerid, int high = 0, int low = 0)
        {
            var cc = new CustomerIdCompare();
            var entry = new Entry { CustomerID = customerid };
            if (Leaderboard == null)
                return null;
            int ret = Leaderboard.BinarySearch(entry, cc);
            var list = Leaderboard?.GetRange(ret - low, ret + high);
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