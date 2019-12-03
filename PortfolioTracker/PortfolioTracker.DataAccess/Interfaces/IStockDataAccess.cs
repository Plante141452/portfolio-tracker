using PortfolioTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortfolioTracker.DataAccess.Interfaces
{
    public interface IStockDataAccess
    {
        Task<List<Quote>> GetQuotes();
        Task SaveQuotes(List<Quote> quotes);
        Task<StockHistory> GetHistory(string symbol);
        Task SaveHistory(StockHistory history);
    }
}