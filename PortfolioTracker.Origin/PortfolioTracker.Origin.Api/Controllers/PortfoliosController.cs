using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.Common.Models.Enums;

namespace PortfolioTracker.Origin.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // GET api/values/5
        [HttpGet("{user}/Portfolios")]
        public ActionResult<List<Portfolio>> Get(int id)
        {
            List<Portfolio> portfolios = new List<Portfolio>
            {
                new Portfolio
                {
                    Allocations = new List<StockAllocation>
                    {
                        new StockAllocation { Symbol = "SPY", AllocationType = AllocationTypeEnum.Percentage, AllocationAmount = 50 },
                        new StockAllocation { Symbol = "QQQ", AllocationType = AllocationTypeEnum.Percentage, AllocationAmount = 50 }
                    }
                }
            };
            return new ActionResult<List<Portfolio>>(portfolios);
        }
    }
}