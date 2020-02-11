using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Origin.AlphaClient;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.DataAccess;
using PortfolioTracker.Origin.IEX;

namespace PortfolioTracker.Origin.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly AlphaClientLogic _alphaClientLogic;

        public StocksController()
        {
            var alphaClient = new AlphaClientWrapper(new AlphaVantageStocksClientFactory(new ApiKeyProvider()));
            var iexClient = new IEXClient();
            var stockData = new StockDataAccess(new MongoClientWrapper(new ConnectionStringProvider()));
            _alphaClientLogic = new AlphaClientLogic(alphaClient, iexClient, stockData);
        }

        // GET api/values/5
        [HttpGet]
        public ActionResult<List<StockHistory>> Get(string symbols)
        {
            {
                var quoteSymbols = symbols.Split(',').ToList();
                return _alphaClientLogic.GetHistory(quoteSymbols).GetAwaiter().GetResult();
            }
        }
    }
}