using Microsoft.AspNetCore.Mvc;
using LeaderboardApi.Models;
using LeaderboardApi.Interfaces;

namespace LeaderboardApi.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpPost]
        [Route("/customer/{Customerid:int}/score/{Score:decimal}")]
        public async Task<IActionResult> Update(Int64 customerid, Decimal score)
        {
            if (score < -1000 || score > 1000)
            {
                throw new ProblemException("value of parameter 'Score' should be in [-1000 ~ 1000].", "Invalid value of parameter 'Score'.");
            }
            var entry = new Entry
            {
                CustomerID = customerid,
                Score = score
            };
            _leaderboardService.EnqueueEntry(entry);

            var response = await _leaderboardService.ProcessUpdateQueueAsync();

            return Ok(response);
        }

        [HttpGet]
        public async Task<IEnumerable<Entry>> GetCustomerByRank([FromQuery]int start, [FromQuery]int end)
        {
            var list = new List<Entry>();

            await Task.Run(() => 
            {
                list = _leaderboardService.GetCustomerByRank(start, end);
            });

            if (list == null)
            {
                throw new ProblemException("Leaderboard has not initialize yet.", "Uninitialilze issue");
            }

            return list.Select(p => new Entry 
            {
                CustomerID= p.CustomerID,
                Score = p.Score,
                Rank = p.Rank
            }).ToArray();
        }

        [HttpGet("{customerid:long}")]
        public async Task<IEnumerable<Entry>> GetCustomerByCustomerID(Int64 customerid, [FromQuery]int high, [FromQuery]int low)
        {
            var list = new List<Entry>();

            await Task.Run(() =>
            {
                list = _leaderboardService.GetCustomerByCustomerID(customerid, high, low);
            });
            if (list == null)
            {
                throw new ProblemException("Leaderboard has not initialize yet.", "Uninitialilze issue");
            }
            return list.ToArray();
        }
    }
}
