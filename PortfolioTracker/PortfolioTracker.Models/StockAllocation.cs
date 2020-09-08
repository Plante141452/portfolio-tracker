using PortfolioTracker.Models.Enums;

namespace PortfolioTracker.Models
{
    public class StockAllocation
    {
        private double _desiredAmount;
        public string Symbol { get; set; }
        public AllocationTypeEnum DesiredAmountType { get; set; }

        public double DesiredAmount
        {
            get => _desiredAmount;
            set
            {
                _desiredAmount = value;
                MinAmount = value * .75;
                MaxAmount = value * 1.25;
            }
        }

        public double MinAmount { get; private set; }
        public double MaxAmount { get; private set; }
        public int CurrentShares { get; set; }
        public double PurchaseRange { get; set; }
    }
}