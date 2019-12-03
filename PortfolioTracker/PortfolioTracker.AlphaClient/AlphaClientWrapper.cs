using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AlphaVantage.Net.Stocks;
using PortfolioTracker.AlphaClient.Interfaces;

namespace PortfolioTracker.AlphaClient
{
    public class AlphaClientWrapper : IAlphaClientWrapper
    {
        private readonly IAlphaVantageStocksClientFactory _clientFactory;

        public AlphaClientWrapper(IAlphaVantageStocksClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private const int RequestsPerMinute = 5;
        private static readonly ConcurrentQueue<DateTimeOffset> RequestTimeLog = new ConcurrentQueue<DateTimeOffset>();
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
                        RequestTimeLog.TryPeek(out var lastRequest);
                        var elapsedTime = DateTimeOffset.UtcNow.Subtract(lastRequest).TotalSeconds;
                        var wait = 65 - elapsedTime;
                        if (wait > 0)
                            throw new InvalidOperationException("Unable to accept request at this time, please requeue.");
                        RequestTimeLog.TryDequeue(out lastRequest);
                    }

                    RequestTimeLog.Enqueue(DateTimeOffset.UtcNow);
                }

                var client = _clientFactory.GetClient();

                try
                {
                    return func(client);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                return default(T);
            });
        }
    }
}