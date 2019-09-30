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
    public class QuotesController : ControllerBase
    {
        private readonly AlphaClientLogic _alphaClientLogic;

        public QuotesController()
        {
            var clientFactory = new AlphaVantageStocksClientFactory(new ApiKeyProvider());
            //var iexClient = new IEXClient();
            _alphaClientLogic = new AlphaClientLogic(new AlphaClientWrapper(clientFactory), new StockDataAccess(new MongoClientWrapper(new ConnectionStringProvider())));
        }

        // GET api/values/5
        [HttpGet]
        public ActionResult<List<Quote>> Get(string symbols)
        {
            {
                var quoteSymbols = symbols.Split(',').ToList();
                return _alphaClientLogic.GetQuotes(quoteSymbols).GetAwaiter().GetResult();
            }
        }
    }
}