﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Client;
using PortfolioTracker.Models;

namespace PortfolioTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly IStockApiWrapper _stockApiWrapper;

        public StocksController()
        {
            _stockApiWrapper = new StockApiWrapper();
        }

        // GET api/values/5
        [HttpGet]
        public ActionResult<List<StockHistory>> Get(string symbols)
        {
            {
                var quoteSymbols = symbols.Split(',').ToList();
                return _stockApiWrapper.GetHistory(quoteSymbols).GetAwaiter().GetResult();
            }
        }
    }
}