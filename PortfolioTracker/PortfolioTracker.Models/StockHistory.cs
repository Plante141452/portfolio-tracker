using System.Collections.Generic;

namespace PortfolioTracker.Models
{
    public class StockHistory
    {
        public string Symbol { get; set; }
        public List<StockHistoryItem> History { get; set; }
    }
}
