using System.Collections.Generic;
using System.Threading.Tasks;
using PortfolioTracker.Models;

namespace PortfolioTracker.Client
{
    public interface IStockApiWrapper
    {
        Task<List<StockHistory>> GetHistory(List<string> symbols);
        Task<List<PortfolioHistoryPeriod>> GetPortfolioHistory(List<string> symbols);
        Task<List<Quote>> GetQuotes(List<string> symbols);
        Task<List<dynamic>> GetMetrics(List<string> quoteSymbols);
    }
}