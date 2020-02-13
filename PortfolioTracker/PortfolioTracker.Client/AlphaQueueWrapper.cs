using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlphaVantage.Net.Stocks.TimeSeries;
using PortfolioTracker.AlphaClient;
using PortfolioTracker.AlphaClient.Interfaces;
using PortfolioTracker.DataAccess;
using PortfolioTracker.DataAccess.Interfaces;
using PortfolioTracker.Models;

namespace PortfolioTracker.Client
{
    public interface IAlphaQueueWrapper
    {
        void StartQueueListener();
    }

    public class AlphaQueueWrapper : IAlphaQueueWrapper
    {
        public const string UpdateWeeklyEvent = "UpdateWeekly";

        public readonly IQueueWrapper AlphaQueue;

        private readonly IAlphaClientWrapper _alphaClient;
        private readonly IStockDataAccess _stockDataAccess;

        public AlphaQueueWrapper()
            : this(new QueueWrapper("alpha-vantage-events"), new AlphaClientWrapper(), new StockDataAccess())
        {
        }

        public AlphaQueueWrapper(IQueueWrapper alphaQueue, IAlphaClientWrapper alphaClient, IStockDataAccess stockDataAccess)
        {
            AlphaQueue = alphaQueue;
            _alphaClient = alphaClient;
            _stockDataAccess = stockDataAccess;
        }

        private const int RequestsPerMinute = 5;
        private static readonly ConcurrentQueue<DateTimeOffset> RequestTimeLog = new ConcurrentQueue<DateTimeOffset>();
        private static readonly object RequestWaitLock = new object();

        public void StartQueueListener()
        {
            AlphaQueue.HandleMessages(async message =>
            {
                lock (RequestWaitLock)
                {
                    if (RequestTimeLog.Count >= RequestsPerMinute)
                    {
                        RequestTimeLog.TryPeek(out var lastRequest);
                        var requestTime = lastRequest.AddMinutes(1.1);

                        var delayTime = requestTime.Subtract(DateTimeOffset.UtcNow);

                        if (delayTime.TotalMilliseconds > 10)
                        {
                            AlphaQueue.QueueMessage(message, delayTime).GetAwaiter().GetResult();
                            RequestTimeLog.Enqueue(requestTime);
                            return;
                        }

                        RequestTimeLog.TryDequeue(out _);
                    }

                    RequestTimeLog.Enqueue(DateTimeOffset.UtcNow);
                }

                switch (message.EventType)
                {
                    case UpdateWeeklyEvent:
                        await UpdateWeekly(message.Content);
                        break;
                    default:
                        throw new Exception("Invalid Event Type!");
                }
            }, ex => throw ex);
        }

        #region UpdateWeekly

        private async Task UpdateWeekly(string stockSymbol)
        {
            await GetHistory(stockSymbol);
        }

        private async Task GetHistory(string symbol)
        {
            ConcurrentQueue<StockHistory> histories = new ConcurrentQueue<StockHistory>();

            var existingData = await _stockDataAccess.GetHistory(symbol);

            var data = await _alphaClient.Execute(svc => svc.RequestWeeklyTimeSeriesAsync(symbol, true));

            var history = TransformSeries(data);

            if (existingData != null)
                history = MergeHistories(history, existingData);

            histories.Enqueue(history);
            await _stockDataAccess.SaveHistory(history);
        }

        private StockHistory MergeHistories(StockHistory updatedData, StockHistory existingData)
        {
            var existingDataToMerge = existingData.History.Where(h => !updatedData.History.Any(uh => uh.ClosingDate.Date.Equals(h.ClosingDate.Date))).ToList();

            if (existingDataToMerge.Count == existingData.History.Count)
                throw new Exception("Cannot merge historical data as they do not overlap.");

            updatedData.History.AddRange(existingDataToMerge);
            updatedData.History = updatedData.History.OrderByDescending(h => h.ClosingDate).ToList();

            StockHistoryItem lastClosing = updatedData.History.First();
            foreach (var history in updatedData.History.Skip(1))
            {
                history.AdjustedClose = lastClosing.AdjustedPercentChanged > 0 ? lastClosing.AdjustedClose / lastClosing.AdjustedPercentChanged : lastClosing.AdjustedClose;
                lastClosing = history;
            }

            return updatedData;
        }

        private StockHistory TransformSeries(StockTimeSeries series)
        {
            List<StockDataPoint> history = series.DataPoints.ToList();

            if (DateTimeOffset.UtcNow.Subtract(history.First().Time).TotalDays < 1)
                history = history.Skip(1).ToList();

            return new StockHistory
            {
                Symbol = series.Symbol,
                History = history.Select((dp, i) => new StockHistoryItem
                {
                    ClosingDate = dp.Time,
                    Volume = dp.Volume,
                    AdjustedClose = dp.ClosingPrice,
                    AdjustedPercentChanged = i == history.Count - 1 ? 0 : dp.ClosingPrice / history[i + 1].ClosingPrice
                }).ToList()
            };
        }

        #endregion
    }
}