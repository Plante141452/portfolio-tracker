using System.Threading.Tasks;
using PortfolioTracker.Models;

namespace PortfolioTracker.Client.Interfaces
{
    public interface IStockClient
    {
        Task<ReturnObject<StockHistory>> GetHistory(string symbol);
    }
}