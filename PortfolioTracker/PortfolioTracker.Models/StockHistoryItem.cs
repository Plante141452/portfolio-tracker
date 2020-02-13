using System;

namespace PortfolioTracker.Models
{
    public class StockHistoryItem
    {
        public DateTimeOffset ClosingDate { get; set; }
        public long Volume { get; set; }
        public decimal AdjustedClose { get; set; }
        public decimal AdjustedPercentChanged { get; set; }
    }
}