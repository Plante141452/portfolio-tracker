using System.Collections.Generic;
using System.Threading.Tasks;
using PortfolioTracker.Logic.Models;

namespace PortfolioTracker.Logic
{
    public interface IRebalanceLogic
    {
        Task<List<SimulationResult>> Simulate(RunScenarioDataSet dataSet);
        Task<List<ScenarioResult>> RunScenario(RunScenarioDataSet dataSet);
        RebalanceResult Rebalance(RebalanceDataSet dataSet, bool quickRebalance = true);
    }
}