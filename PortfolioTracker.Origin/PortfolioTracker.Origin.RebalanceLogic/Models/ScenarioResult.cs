using System;

namespace PortfolioTracker.Origin.RebalanceLogic.Models
{
    public class ScenarioResult
    {
        public decimal PercentIncrease { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public decimal FinalPortfolioValue { get; set; }
        public decimal TotalCashInvested { get; set; }
    }
}