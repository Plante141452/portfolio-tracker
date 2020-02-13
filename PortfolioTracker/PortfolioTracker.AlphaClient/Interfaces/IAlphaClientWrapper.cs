using System;
using System.Threading.Tasks;
using PortfolioTracker.AlphaClient.AlphaVantage.Net.AlphaVantage.Net.Stocks;

namespace PortfolioTracker.AlphaClient.Interfaces
{
    public interface IAlphaClientWrapper
    {
        Task<T> Execute<T>(Func<AlphaVantageStocksClient, Task<T>> task);
    }
}