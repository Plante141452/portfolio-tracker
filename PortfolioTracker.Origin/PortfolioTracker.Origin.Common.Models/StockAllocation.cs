using PortfolioTracker.Origin.Common.Models.Enums;

namespace PortfolioTracker.Origin.Common.Models
{
    public class StockAllocation
    {
        public string Symbol { get; set; }
        public AllocationTypeEnum DesiredAmountType { get; set; }
        public decimal DesiredAmount { get; set; }
        public int CurrentShares { get; set; }
    }
}