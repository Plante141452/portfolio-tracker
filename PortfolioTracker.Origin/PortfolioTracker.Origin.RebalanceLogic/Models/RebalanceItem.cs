using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.RebalanceLogic.Models
{
    public class RebalanceItem
    {
        public StockAllocation ActualAllocation { get; set; }
        public StockAllocation DesiredAllocation { get; set; }
        public decimal Price { get; set; }
    }
}