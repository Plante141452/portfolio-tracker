using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Client;
using PortfolioTracker.Models;

namespace PortfolioTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {
        private readonly IStockApiWrapper _alphaClientLogic;

        public QuotesController()
        {
            _alphaClientLogic = new StockApiWrapper();
        }

        // GET api/values/5
        [HttpGet]
        public async Task<ActionResult<List<Quote>>> Get(string symbols)
        {
            {
                var quoteSymbols = symbols.Split(',').ToList();
                return await _alphaClientLogic.GetQuotes(quoteSymbols);
            }
        }
    }
}