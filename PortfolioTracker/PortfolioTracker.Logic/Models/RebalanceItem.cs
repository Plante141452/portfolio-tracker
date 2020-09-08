using PortfolioTracker.Models;

namespace PortfolioTracker.Logic.Models
{
    public class RebalanceItem : StockAllocation
    {
        public double Price { get; set; }
    }
}