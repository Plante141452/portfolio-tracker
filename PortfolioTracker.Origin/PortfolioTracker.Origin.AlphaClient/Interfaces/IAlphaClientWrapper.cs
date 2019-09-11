using System;
using System.Threading.Tasks;
using AlphaVantage.Net.Stocks;

namespace PortfolioTracker.Origin.AlphaClient.Interfaces
{
    public interface IAlphaClientWrapper
    {
        Task<T> Execute<T>(Func<AlphaVantageStocksClient, T> func);
        Task<T> Execute<T>(Func<AlphaVantageStocksClient, Task<T>> task);
    }
}