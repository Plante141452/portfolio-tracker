using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.RebalanceLogic.Models
{
    public class RebalanceItem : StockAllocation
    {
        public decimal Price { get; set; }
    }
}