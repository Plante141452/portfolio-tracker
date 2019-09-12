using System.Collections.Generic;
using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.RebalanceLogic
{
    public class SimulationResult
    {
        public Portfolio Portfolio { get; set; }
        public List<ScenarioResult> Results { get; set; }
    }
}