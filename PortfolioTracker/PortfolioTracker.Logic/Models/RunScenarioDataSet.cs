using PortfolioTracker.Models;
using PortfolioTracker.Models.Enums;
using System.Collections.Generic;

namespace PortfolioTracker.Logic.Models
{
    public class RunScenarioDataSet
    {
        public List<Portfolio> Portfolios { get; set; }
        public List<PortfolioHistoryPeriod> History { get; set; }
        public double InitialInvestment { get; set; }

        public CadenceTypeEnum CashInfluxCadence { get; set; }
        public double CashInfluxAmount { get; set; }
    }
}