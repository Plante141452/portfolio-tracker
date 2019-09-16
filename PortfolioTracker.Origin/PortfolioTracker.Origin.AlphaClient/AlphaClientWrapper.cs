using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AlphaVantage.Net.Stocks;
using PortfolioTracker.Origin.AlphaClient.Interfaces;

namespace PortfolioTracker.Origin.AlphaClient
{
    public class AlphaClientWrapper : IAlphaClientWrapper
    {
        private readonly Lazy<AlphaVantageStocksClient> _clientLazy;

        private AlphaVantageStocksClient Client => _clientLazy.Value;

        public AlphaClientWrapper(IApiKeyProvider apiKeyProvider)
        {
            _clientLazy = new Lazy<AlphaVantageStocksClient>(() => new AlphaVantageStocksClient(apiKeyProvider.AlphaVantageKey));
        }

        private const int RequestsPerMinute = 5;
        private static readonly ConcurrentQueue<DateTime> RequestTimeLog = new ConcurrentQueue<DateTime>();
        private static readonly object RequestWaitLock = new object();

        public async Task<T> Execute<T>(Func<AlphaVantageStocksClient, Task<T>> task)
        {
            return await await Execute<Task<T>>(task);
        }

        public async Task<T> Execute<T>(Func<AlphaVantageStocksClient, T> func)
        {
            return await Task.Run(() =>
            {
                lock (RequestWaitLock)
                {
                    if (RequestTimeLog.Count >= RequestsPerMinute)
                    {
                        RequestTimeLog.TryDequeue(out var lastRequest);
                        double elapsedTime = DateTime.Now.Subtract(lastRequest).TotalSeconds;
                        double wait = 65 - elapsedTime;
                        if (wait > 0)
                            Thread.Sleep((int)(wait * 1000));
                    }

                    RequestTimeLog.Enqueue(DateTime.Now);
                }

                return func(Client);
            });
        }
    }
}