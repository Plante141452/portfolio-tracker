using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Client;
using PortfolioTracker.Models;

namespace PortfolioTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {
        private readonly IStockApiWrapper _alphaClientLogic;

        public QuotesController()
        {
            _alphaClientLogic = new StockApiWrapper();
        }

        // GET api/values/5
        [HttpGet]
        public async Task<ActionResult<ReturnObject<List<Quote>>>> Get(string symbols)
        {
            {
                var ret = new ReturnObject<List<Quote>> { Success = true };

                try
                {
                    var quoteSymbols = symbols.Split(',').ToList();
                    ret.Data = await _alphaClientLogic.GetQuotes(quoteSymbols);
                }
                catch (Exception ex)
                {
                    ret.Success = false;
                    ret.Messages = new List<ReturnMessage> {new ReturnMessage { MessageType = MessageTypeEnum.Error, Message = ex.Message} };
                }

                return ret;
            }
        }
    }
}