using System;
using System.Collections.Generic;

namespace PortfolioTracker.Models
{
    public class PortfolioHistoryPeriod
    {
        public DateTimeOffset ClosingDate { get; set; }
        public List<StockHistoricalPeriod> Stocks { get; set; }
    }
}
