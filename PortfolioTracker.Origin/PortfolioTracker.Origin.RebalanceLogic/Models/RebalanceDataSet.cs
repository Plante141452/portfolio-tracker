﻿using System.Collections.Generic;
using PortfolioTracker.Origin.AlphaClient;
using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.RebalanceLogic.Models
{
    public class RebalanceDataSet
    {
        public Portfolio Portfolio { get; set; }
        public List<StockAllocation> ActualAllocations { get; set; }
        public List<Quote> Quotes { get; set; }
        public decimal CashOnHand { get; set; }
    }
}