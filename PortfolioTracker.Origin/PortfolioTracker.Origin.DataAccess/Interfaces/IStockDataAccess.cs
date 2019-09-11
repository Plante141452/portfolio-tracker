using System.Threading.Tasks;
using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.DataAccess
{
    public interface IStockDataAccess
    {
        Task<StockHistory> GetHistory(string symbol);
        Task SaveHistory(StockHistory history);
    }
}