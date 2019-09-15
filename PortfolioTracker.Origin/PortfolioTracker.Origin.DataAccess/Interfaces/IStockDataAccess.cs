using System.Collections.Generic;
using System.Threading.Tasks;
using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.DataAccess.Interfaces
{
    public interface IStockDataAccess
    {
        Task<List<Quote>> GetQuotes();
        Task SaveQuotes(List<Quote> quotes);
        Task<StockHistory> GetHistory(string symbol);
        Task SaveHistory(StockHistory history);
    }
}