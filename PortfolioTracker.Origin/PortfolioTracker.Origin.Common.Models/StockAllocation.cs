using PortfolioTracker.Origin.Common.Models.Enums;

namespace PortfolioTracker.Origin.Common.Models
{
    public class StockAllocation
    {
        public string Symbol { get; set; }
        public AllocationTypeEnum AllocationType { get; set; }
        public decimal AllocationAmount { get; set; }
    }
}