using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using SortingGame.Models;
using SortingGame.Models.Repository;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;

namespace SortingGame.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class LeaderboardController : ControllerBase
    {

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
            decimal UpdatedEntry = 0;
            await Task.Run(() =>
            {
                UpdatedEntry = LeaderboardRepository.Update(entry);
            });
            return Ok(UpdatedEntry);
        }

        [HttpGet]
        public async Task<IEnumerable<Entry>> GetCustomerByRank([FromQuery]int start, [FromQuery]int end)
        {
            var list = new List<Entry>();

            await Task.Run(() => 
            {
                list = LeaderboardRepository.GetCustomerByRank(start, end);
            });

            if (list == null)
            {
                throw new ProblemException("Leaderboard has not initialize yet.", "Uninitialilze issue");
            }

            return list.Select(p => new Entry 
            {
                CustomerID=p.CustomerID,
                Score = p.Score,
                Rank = p.Rank
            }).ToArray();
        }

        [HttpGet("{customerid:long}")]
        public async Task<IActionResult> GetCustomerByCustomerID(Int64 customerid, [FromQuery]int high, [FromQuery]int low)
        {
            var list = new List<Entry>();

            await Task.Run(() =>
            {
                list = LeaderboardRepository.GetCustomerByCustomerID(customerid, high, low);
            });
            if (list == null)
            {
                throw new ProblemException("Leaderboard has not initialize yet.", "Uninitialilze issue");
            }
            return Ok(list.ToArray());
        }
    }
}
