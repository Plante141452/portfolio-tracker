using System.Collections.Generic;
using System.Threading.Tasks;
using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.AlphaClient.Interfaces
{
    public interface IAlphaClientLogic
    {
        Task<List<StockHistory>> GetHistory(List<string> symbols);
        Task<List<PortfolioHistoryPeriod>> GetPortfolioHistory(List<string> symbols);
        Task<List<Quote>> GetQuotes(List<string> symbols);
    }
}