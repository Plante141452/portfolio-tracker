using System.Collections.Generic;
using PortfolioTracker.Models;
using PortfolioTracker.Models.Enums;

namespace PortfolioTracker.Logic.Models
{
    public class RunScenarioDataSet
    {
        public List<Portfolio> Portfolios { get; set; }
        public List<PortfolioHistoryPeriod> History { get; set; }
        public decimal InitialInvestment { get; set; }

        public CadenceTypeEnum CashInfluxCadence { get; set; }
        public decimal CashInfluxAmount { get; set; }
    }
}