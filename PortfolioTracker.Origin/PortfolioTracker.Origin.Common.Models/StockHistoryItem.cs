using System;

namespace PortfolioTracker.Origin.Common.Models
{
    public class StockHistoryItem
    {
        public DateTime ClosingDate { get; set; }
        public decimal AdjustedClose { get; set; }
        public decimal AdjustedPercentChanged { get; set; }
    }
}