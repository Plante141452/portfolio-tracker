using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Origin.AlphaClient;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.DataAccess;

namespace PortfolioTracker.Origin.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly AlphaClientLogic _alphaClientLogic;

        public StocksController()
        {
            var clientFactory = new AlphaVantageStocksClientFactory(new ApiKeyProvider());
            _alphaClientLogic = new AlphaClientLogic(new AlphaClientWrapper(clientFactory), new StockDataAccess(new MongoClientWrapper(new ConnectionStringProvider())));
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