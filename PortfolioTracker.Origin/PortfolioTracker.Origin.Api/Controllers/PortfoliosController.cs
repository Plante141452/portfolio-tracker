using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Origin.AlphaClient;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.DataAccess;
using PortfolioTracker.Origin.DataAccess.Interfaces;
using PortfolioTracker.Origin.RebalanceLogic.Interfaces;
using PortfolioTracker.Origin.RebalanceLogic.Models;

namespace PortfolioTracker.Origin.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfoliosController : ControllerBase
    {
        private readonly AlphaClientLogic _alphaClientLogic;
        private readonly IRebalanceLogic _rebalanceLogic;
        private readonly IPortfolioDataAccess _portfolioDataAccess;

        public PortfoliosController()
        {
            var mongoWrapper = new MongoClientWrapper(new ConnectionStringProvider());
            var clientFactory = new AlphaVantageStocksClientFactory(new ApiKeyProvider());
            //var iexClient = new IEXClient();

            _portfolioDataAccess = new PortfolioDataAccess(mongoWrapper);
            _alphaClientLogic = new AlphaClientLogic(new AlphaClientWrapper(clientFactory), new StockDataAccess(mongoWrapper));
            _rebalanceLogic = new RebalanceLogic.RebalanceLogic();
        }

        //5d80d0587d2d4657d8e1fe8f
        [HttpGet("{id}")]
        public ActionResult<Portfolio> Get(string id)
        {
            {
                return _portfolioDataAccess.GetPortfolio(id).GetAwaiter().GetResult();
            }
        }

        [HttpPut("{id}")]
        public ActionResult<Portfolio> Put(string id, [FromBody] Portfolio portfolio)
        {
            {
                var portfolios = new List<Portfolio> { portfolio };
                var results = _portfolioDataAccess.SavePortfolios(portfolios).GetAwaiter().GetResult();
                return results.FirstOrDefault();
            }
        }

        //5d80d0587d2d4657d8e1fe8f
        [HttpGet("{id}/rebalance")]
        public ActionResult<RebalanceResult> Rebalance(string id)
        {
            {
                Portfolio portfolio = _portfolioDataAccess.GetPortfolio(id).GetAwaiter().GetResult();

                var symbols = portfolio.AllStocks.Select(s => s.Symbol).ToList();
                var quotes = _alphaClientLogic.GetQuotes(symbols).GetAwaiter().GetResult();

                var dataSet = new RebalanceDataSet
                {
                    Portfolio = portfolio,
                    Quotes = quotes
                };

                return _rebalanceLogic.Rebalance(dataSet, false);
            }
        }
    }
}