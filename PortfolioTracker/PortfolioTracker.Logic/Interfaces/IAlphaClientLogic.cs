using PortfolioTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortfolioTracker.AlphaClient.Interfaces
{
    public interface IAlphaClientLogic
    {
        Task<StockHistory> GetHistory(string symbol);
        Task<List<PortfolioHistoryPeriod>> GetPortfolioHistory(List<string> symbols);
        Task<List<Quote>> GetQuotes(List<string> symbols);
    }
}