using Microsoft.AspNetCore.Mvc;
using LeaderboardApi.Models;
using LeaderboardApi.Interfaces;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace LeaderboardApi.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class LeaderboardController(ILeaderboardService leaderboardService) : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService = leaderboardService;

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

            var response = await _leaderboardService.EnqueueEntry(entry);

            return Ok(response);
        }

        [HttpGet]
        public async Task<IEnumerable<Entry>> GetCustomerByRank([FromQuery]int start, [FromQuery]int end)
        {
            var list = new List<Entry>();

            if (end - start < 0)
                throw new ProblemException("End rank less than start rank.", "Invalid parameter value");

            await Task.Run(() =>
            {
                list = _leaderboardService.GetCustomerByRank(start, end);
            });

            return [.. list.Select(p => new Entry 
            {
                CustomerID = p.CustomerID,
                Score = p.Score,
                Rank = p.Rank
            })];
        }
        
        [HttpGet("/{customerid:long:required}")]
        public async Task<IEnumerable<Entry>> GetCustomerByCustomerID(Int64 customerid, [FromQuery]int high, [FromQuery]int low)
        {
            var list = new List<Entry>();

            await Task.Run(() =>
            {
                list = _leaderboardService.();
            });
            if (list == null)
                throw new ProblemException("Leaderboard has not initialize yet.", "Uninitialilze issue");
                
            return [.. list];
        }

        // note(wangjw): Test
        [HttpGet("[action]")]
        public async Task<IEnumerable<Entry>> GetAll()
        {
            var list = new List<Entry>();

            await Task.Run(() =>
            {
                list = _leaderboardService.GetAll();
            });
            
            return [.. list.Select(p => new Entry
            {
                CustomerID = p.CustomerID,
                Score = p.Score,
                Rank = p.Rank
            })];
        }

        // note(wangjw): Test
        [HttpGet("[action]")]
        public async Task<Entry> GetEntryById([FromQuery] Int64 customerid)
        {
            var entry = new Entry();

            await Task.Run(() =>
            {
                entry = _leaderboardService.GetEntryById(customerid);
            });

            return entry;
        }
    }
}
