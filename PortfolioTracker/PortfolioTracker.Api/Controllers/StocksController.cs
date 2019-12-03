using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.AlphaClient;
using PortfolioTracker.AlphaClient.Interfaces;
using PortfolioTracker.DataAccess;
using PortfolioTracker.Models;

namespace PortfolioTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly IAlphaClientLogic _alphaClientLogic;

        public StocksController()
        {
            var clientFactory = new AlphaVantageStocksClientFactory(new ApiKeyProvider());
            //var iexClient = new IEXClient();
            _alphaClientLogic = new AlphaClientLogic(new AlphaClientWrapper(clientFactory), new StockDataAccess(new MongoClientWrapper(new ConnectionStringProvider())));
        }

        // GET api/values/5
        [HttpGet]
        public ActionResult<ReturnObject<StockHistory>> Get(string symbol)
        {
            {
                try
                {
                    var data = _alphaClientLogic.GetHistory(symbol).GetAwaiter().GetResult();
                    return new ReturnObject<StockHistory>
                    {
                        Success = true,
                        Data = data
                    };
                }
                catch(Exception ex)
                {
                    return new ReturnObject<StockHistory>
                    {
                        Success = false,
                        Messages = new List<ReturnMessage>
                        {
                            new ReturnMessage
                            {
                                Message = ex.Message,
                                MessageType = MessageTypeEnum.Error
                            }
                        }
                    };
                }
            }
        }
    }
}