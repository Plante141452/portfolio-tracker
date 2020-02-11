using PortfolioTracker.Origin.Common.Models.Enums;

namespace PortfolioTracker.Origin.Common.Models
{
    public class StockAllocation
    {
        private decimal _desiredAmount;
        public string Symbol { get; set; }
        public AllocationTypeEnum DesiredAmountType { get; set; }

        public decimal DesiredAmount
        {
            get => _desiredAmount;
            set
            {
                _desiredAmount = value;
                MinAmount = value * .75m;
                MaxAmount = value * 1.25m;
            }
        }

        public decimal MinAmount { get; private set; }
        public decimal MaxAmount { get; private set; }
        public int CurrentShares { get; set; }
    }
}