using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.AlphaClient;
using PortfolioTracker.AlphaClient.Interfaces;
using PortfolioTracker.DataAccess;
using PortfolioTracker.Logic;
using PortfolioTracker.Logic.Interfaces;
using PortfolioTracker.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortfolioTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly IAlphaClientLogic _alphaClientLogic;
        private readonly IAzureQueueClient _azureQueueClient;

        public StocksController()
        {
            var clientFactory = new AlphaVantageStocksClientFactory(new ApiKeyProvider());
            
            //var iexClient = new IEXClient();
            _alphaClientLogic = new AlphaClientLogic(new AlphaClientWrapper(clientFactory), new StockDataAccess(new MongoClientWrapper(new ConnectionStringProvider())));
            _azureQueueClient = new AzureQueueClient();
        }

        // GET api/values/5
        [HttpGet]
        public async Task<ActionResult<ReturnObject<StockHistory>>> Get(string symbol)
        {
            {
                try
                {
                    var data = await _alphaClientLogic.GetHistory(symbol);
                    return new ReturnObject<StockHistory>
                    {
                        Success = true,
                        Data = data
                    };
                }
                catch
                {
                    await _azureQueueClient.SendMessageAsync(AzureQueueClient.UpdateHistoryQueue, symbol, 90000);
                    return new ReturnObject<StockHistory>
                    {
                        Success = false
                    };
                }
            }
        }
    }
}