using System;

namespace PortfolioTracker.Logic.Models
{
    public class RebalancePercentageItem : RebalanceItem
    {
        public double DesiredFactor { get; set; }
        public double CurrentFactor { get; set; }
        public int EndAmount { get; set; }
        public double FactorDifference => Math.Abs(DesiredFactor - CurrentFactor);
    }
}