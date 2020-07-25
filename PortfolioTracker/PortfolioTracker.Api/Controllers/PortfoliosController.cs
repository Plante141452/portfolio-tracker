using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<ActionResult<ReturnObject<Portfolio>>> Get(string id)
        {
            {
                var ret = new ReturnObject<Portfolio> { Success = true };

                try
                {
                    ret.Data = await _portfolioDataAccess.GetPortfolio(id);
                }
                catch (Exception ex)
                {
                    ret.Success = false;
                    ret.Messages = new List<ReturnMessage> {new ReturnMessage{ MessageType = MessageTypeEnum.Error, Message = ex.Message}};
                }

                return ret;
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ReturnObject<Portfolio>>> Put(string id, [FromBody] Portfolio portfolio)
        {
            {

                var ret = new ReturnObject<Portfolio> { Success = true };

                try
                {
                    var portfolios = new List<Portfolio> { portfolio };
                    var results =await  _portfolioDataAccess.SavePortfolios(portfolios);
                    ret.Data =results.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    ret.Success = false;
                    ret.Messages = new List<ReturnMessage> {new ReturnMessage{ MessageType = MessageTypeEnum.Error, Message = ex.Message}};
                }

                return ret;
            }
        }

        //5d80d0587d2d4657d8e1fe8f
        [HttpGet("{id}/rebalance")]
        public async Task<ActionResult<ReturnObject<RebalanceResult>>> Rebalance(string id)
        {
            {
                var ret = new ReturnObject<RebalanceResult> { Success = true };

                try
                {
                    Portfolio portfolio = await _portfolioDataAccess.GetPortfolio(id);

                    var symbols = portfolio.AllStocks.Select(s => s.Symbol).ToList();
                    var quotes = await _stockApiWrapper.GetQuotes(symbols);

                    var dataSet = new RebalanceDataSet
                    {
                        Portfolio = portfolio,
                        Quotes = quotes
                    };

                    ret.Data = _rebalanceLogic.Rebalance(dataSet, false);
                }
                catch (Exception ex)
                {
                    ret.Success = false;
                    ret.Messages = new List<ReturnMessage> {new ReturnMessage{ MessageType = MessageTypeEnum.Error, Message = ex.Message}};
                }

                return ret;
            }
        }
    }
}