using System.Collections.Generic;
using PortfolioTracker.Models;

namespace PortfolioTracker.Logic.Models
{
    public class SimulationResult
    {
        public Portfolio Portfolio { get; set; }
        public List<ScenarioResult> Results { get; set; }
    }
}