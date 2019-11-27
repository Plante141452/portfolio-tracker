using System.Collections.Generic;
using System.Threading.Tasks;
using PortfolioTracker.Origin.RebalanceLogic.Models;

namespace PortfolioTracker.Origin.RebalanceLogic.Interfaces
{
    public interface IRebalanceLogic
    {
        Task<List<SimulationResult>> Simulate(RunScenarioDataSet dataSet);
        Task<List<ScenarioResult>> RunScenario(RunScenarioDataSet dataSet);
        RebalanceResult Rebalance(RebalanceDataSet dataSet, bool quickRebalance = true);
    }
}