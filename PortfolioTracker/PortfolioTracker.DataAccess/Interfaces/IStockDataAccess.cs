using System.Collections.Generic;
using System.Threading.Tasks;
using PortfolioTracker.DataAccess.DataTypes;
using PortfolioTracker.Models;

namespace PortfolioTracker.DataAccess.Interfaces
{
    public interface IStockDataAccess
    {
        Task<List<Quote>> GetQuotes();
        Task SaveQuotes(List<Quote> quotes);
        Task<StockHistory> GetHistory(string symbol);
        Task SaveHistory(StockHistory history);
        Task<StockMetrics> GetMetrics(string symbol);
        Task SaveMetrics(StockMetrics metrics);
    }
}