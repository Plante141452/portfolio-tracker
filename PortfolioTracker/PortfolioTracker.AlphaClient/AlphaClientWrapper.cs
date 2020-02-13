using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AlphaVantage.Net.Stocks;
using PortfolioTracker.AlphaClient.Interfaces;

namespace PortfolioTracker.AlphaClient
{
    public class AlphaClientWrapper : IAlphaClientWrapper
    {
        private readonly IAlphaVantageStocksClientFactory _clientFactory;

        public AlphaClientWrapper()
            : this(new AlphaVantageStocksClientFactory())
        {
        }

        public AlphaClientWrapper(IAlphaVantageStocksClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<T> Execute<T>(Func<AlphaVantageStocksClient, Task<T>> task)
        {
            return await await Execute<Task<T>>(task);
        }

        public async Task<T> Execute<T>(Func<AlphaVantageStocksClient, T> func)
        {
            return await Task.Run(() =>
            {
                var client = _clientFactory.GetClient();

                try
                {
                    return func(client);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw ex;
                }
            });
        }
    }
}