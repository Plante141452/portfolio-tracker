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
    public class QuotesController : ControllerBase
    {
        private readonly AlphaClientLogic _alphaClientLogic;

        public QuotesController()
        {
            _alphaClientLogic = new AlphaClientLogic(new AlphaClientWrapper(new ApiKeyProvider()), new StockDataAccess(new MongoClientWrapper(new ConnectionStringProvider())));
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