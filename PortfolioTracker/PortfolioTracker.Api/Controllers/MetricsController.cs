using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Client;
using PortfolioTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        private readonly IStockApiWrapper _stockApiWrapper;

        public MetricsController()
        {
            _stockApiWrapper = new StockApiWrapper();
        }

        // GET api/values/5
        [HttpGet]
        public async Task<ActionResult<ReturnObject<List<dynamic>>>> Get(string symbols)
        {
            {
                var ret = new ReturnObject<List<dynamic>> { Success = true };

                try
                {
                    var quoteSymbols = symbols.Split(',').ToList();
                    ret.Data = await _stockApiWrapper.GetMetrics(quoteSymbols);
                }
                catch (Exception ex)
                {
                    ret.Success = false;
                    ret.Messages = new List<ReturnMessage> { new ReturnMessage { MessageType = MessageTypeEnum.Error, Message = ex.Message } };
                }

                return ret;
            }
        }
    }
}