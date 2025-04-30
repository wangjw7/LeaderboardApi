using System.ComponentModel.DataAnnotations;

namespace SortingGame.Models
{
    public class Entry
    {
        public long CustomerID { get; set; }
        
        public decimal Score { get; set; }

        public int Rank { get; set; }
    }
}