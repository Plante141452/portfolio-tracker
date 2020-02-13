using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Client;
using PortfolioTracker.DataAccess;
using PortfolioTracker.DataAccess.Interfaces;
using PortfolioTracker.Logic;
using PortfolioTracker.Logic.Models;
using PortfolioTracker.Models;

namespace PortfolioTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfoliosController : ControllerBase
    {
        private readonly IStockApiWrapper _stockApiWrapper;
        private readonly IRebalanceLogic _rebalanceLogic;
        private readonly IPortfolioDataAccess _portfolioDataAccess;

        public PortfoliosController()
        {
            _portfolioDataAccess = new PortfolioDataAccess();
            _stockApiWrapper = new StockApiWrapper();
            _rebalanceLogic = new RebalanceLogic();
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
                var quotes = _stockApiWrapper.GetQuotes(symbols).GetAwaiter().GetResult();

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