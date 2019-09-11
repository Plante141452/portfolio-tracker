using System.Collections.Generic;
using PortfolioTracker.Origin.AlphaClient;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.Common.Models.Enums;

namespace PortfolioTracker.Origin.RebalanceLogic
{
    public class RunScenarioDataSet
    {
        public Portfolio Portfolio { get; set; }
        public List<PortfolioHistoryPeriod> History { get; set; }
        public decimal InitialInvestment { get; set; }

        public CadenceTypeEnum CashInfluxCadence { get; set; }
        public decimal CashInfluxAmount { get; set; }
    }
}