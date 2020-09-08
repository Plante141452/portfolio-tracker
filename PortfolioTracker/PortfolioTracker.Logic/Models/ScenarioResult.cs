using System;

namespace PortfolioTracker.Logic.Models
{
    public class ScenarioResult
    {
        public double PercentIncrease { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public double FinalPortfolioValue { get; set; }
        public double TotalCashInvested { get; set; }
    }
}