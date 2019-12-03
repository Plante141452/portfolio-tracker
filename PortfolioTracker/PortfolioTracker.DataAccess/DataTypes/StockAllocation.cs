namespace PortfolioTracker.DataAccess.DataTypes
{
    public class StockAllocation
    {
        public string Symbol { get; set; }
        public AllocationTypeEnum DesiredAmountType { get; set; }
        public decimal DesiredAmount { get; set; }
        public int CurrentShares { get; set; }
    }
}