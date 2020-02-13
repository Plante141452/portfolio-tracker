using PortfolioTracker.Models;

namespace PortfolioTracker.Logic.Models
{
    public class RebalanceItem : StockAllocation
    {
        public decimal Price { get; set; }
    }
}