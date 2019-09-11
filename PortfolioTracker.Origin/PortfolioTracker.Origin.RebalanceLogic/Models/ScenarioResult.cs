using System;

namespace PortfolioTracker.Origin.RebalanceLogic
{
    public class ScenarioResult
    {
        public decimal PercentIncrease { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal FinalPortfolioValue { get; set; }
        public decimal TotalCashInvested { get; set; }
    }
}