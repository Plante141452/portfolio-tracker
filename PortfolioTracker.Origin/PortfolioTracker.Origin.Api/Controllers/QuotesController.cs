using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Origin.AlphaClient;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.DataAccess;
using PortfolioTracker.Origin.IEX;

namespace PortfolioTracker.Origin.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {
        private readonly AlphaClientLogic _alphaClientLogic;

        public QuotesController()
        {
            var alphaClient = new AlphaClientWrapper(new AlphaVantageStocksClientFactory(new ApiKeyProvider()));
            var iexClient = new IEXClient();
            var stockData = new StockDataAccess(new MongoClientWrapper(new ConnectionStringProvider()));
            _alphaClientLogic = new AlphaClientLogic(alphaClient, iexClient, stockData);
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