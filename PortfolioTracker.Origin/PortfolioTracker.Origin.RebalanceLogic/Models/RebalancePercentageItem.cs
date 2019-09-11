using System;

namespace PortfolioTracker.Origin.RebalanceLogic.Models
{
    public class RebalancePercentageItem : RebalanceItem
    {
        public decimal DesiredFactor { get; set; }
        public decimal CurrentFactor { get; set; }
        public int EndAmount { get; set; }
        public decimal FactorDifference => Math.Abs(DesiredFactor - CurrentFactor);
    }
}