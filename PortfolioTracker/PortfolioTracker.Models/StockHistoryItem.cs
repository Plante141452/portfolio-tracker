using System;

namespace PortfolioTracker.Models
{
    public class StockHistoryItem
    {
        public DateTimeOffset ClosingDate { get; set; }
        public long Volume { get; set; }
        public double AdjustedClose { get; set; }
        public double AdjustedPercentChanged { get; set; }
    }
}