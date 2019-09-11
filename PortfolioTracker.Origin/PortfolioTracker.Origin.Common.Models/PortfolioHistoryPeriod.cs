using System;
using System.Collections.Generic;

namespace PortfolioTracker.Origin.Common.Models
{
    public class PortfolioHistoryPeriod
    {
        public DateTime ClosingDate { get; set; }
        public List<StockHistoricalPeriod> Stocks { get; set; }
    }
}