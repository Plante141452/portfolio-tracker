using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PortfolioTracker.AlphaClient;
using PortfolioTracker.AlphaClient.Interfaces;
using PortfolioTracker.DataAccess;
using PortfolioTracker.DataAccess.Interfaces;
using PortfolioTracker.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThreeFourteen.AlphaVantage;
using ThreeFourteen.AlphaVantage.Model;

namespace PortfolioTracker.Client
{
    public interface IAlphaQueueWrapper
    {
        void StartQueueListener();
    }

    public class AlphaQueueWrapper : IAlphaQueueWrapper
    {
        public const string UpdateWeeklyEvent = "UpdateWeekly";
        public const string UpdateMetricsEvent = "UpdateMetrics";

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
                try
                {
                    switch (message.EventType)
                    {
                        case UpdateWeeklyEvent:
                            await UpdateWeekly(message);
                            break;
                        case UpdateMetricsEvent:
                            await UpdateMetricsData(message);
                            break;
                        default:
                            throw new Exception("Invalid Event Type!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong: {ex.Message}");
                }
            }, ex => Console.WriteLine(ex.Message));
        }

        private bool ApiCallable(QueueMessage message)
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
                        return false;
                    }

                    RequestTimeLog.TryDequeue(out _);
                }

                RequestTimeLog.Enqueue(DateTimeOffset.UtcNow);
                return true;
            }
        }

        #region UpdateWeekly

        private async Task UpdateWeekly(QueueMessage message)
        {
            string symbol = message.Content;
            var existingData = await _stockDataAccess.GetHistory(symbol);

            var lastRecordedClose = existingData?.History?.Max(h => h.ClosingDate);
            if (lastRecordedClose != null && DateTimeOffset.UtcNow.Subtract(lastRecordedClose.Value).TotalDays < 8)
                return;

            if (ApiCallable(message))
                await GetHistory(symbol, existingData);
        }

        private async Task GetHistory(string symbol, StockHistory existingData)
        {
            Console.WriteLine($"Updated {symbol}");
            var data = await _alphaClient.Execute(svc => svc.Stocks.WeeklyAdjusted(symbol).GetAsync());

            var history = TransformSeries(symbol, data.Data.ToList());

            if (existingData != null)
                history = MergeHistories(history, existingData);

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

        private StockHistory TransformSeries(string symbol, List<TimeSeriesAdjustedEntry> history)
        {
            if (DateTimeOffset.UtcNow.Subtract(history.First().Timestamp).TotalDays < 1)
                history = history.Skip(1).ToList();

            return new StockHistory
            {
                Symbol = symbol,
                History = history.Select((dp, i) => new StockHistoryItem
                {
                    ClosingDate = dp.Timestamp,
                    Volume = dp.Volume,
                    AdjustedClose = dp.AdjustedClose,
                    AdjustedPercentChanged = i == history.Count - 1 ? 0 : dp.AdjustedClose / history[i + 1].AdjustedClose
                }).ToList()
            };
        }

        #endregion

        #region UpdateMetrics

        private async Task UpdateMetricsData(QueueMessage message)
        {
            string symbol = message.Content;
            var existingData = await _stockDataAccess.GetMetrics(symbol);

            if (existingData == null)
                existingData = new StockMetrics { Symbol = symbol };

            var rsiDate = existingData.RsiUpdatedDate;
            var macdDate = existingData.MacdUpdatedDate;

            if ((DateTimeOffset.UtcNow.Subtract(rsiDate).TotalDays > 1) && ApiCallable(message))
                await GetRsi(symbol, existingData);

            if ((DateTimeOffset.UtcNow.Subtract(macdDate).TotalDays > 1) && ApiCallable(message))
                await GetMacd(symbol, existingData);
        }

        private async Task GetRsi(string symbol, StockMetrics metrics)
        {
            Console.WriteLine($"Updated {symbol}");
            var data = await _alphaClient.Execute(svc => svc.Technicals.RelativeStrengthIndex(symbol).SetInterval(Interval.Daily).SetTimePeriod(200).SetSeriesType(SeriesType.Close).GetAsync());

            var rsiScore = GetRsiScore(data.Data.ToList());

            metrics.OverallScore = metrics.OverallScore + rsiScore - metrics.RsiScore;
            metrics.RsiScore = rsiScore;
            metrics.RsiUpdatedDate = DateTimeOffset.UtcNow;

            await _stockDataAccess.SaveMetrics(metrics);
        }

        private double GetRsiScore(List<TechnicalEntry> technicalEntries)
        {
            var minus4DaysScore = .0005 * technicalEntries.Skip(4).First().Value;
            var minus3DaysScore = .0010 * technicalEntries.Skip(3).First().Value;
            var minus2DaysScore = .0020 * technicalEntries.Skip(2).First().Value;
            var minus1DaysScore = .0030 * technicalEntries.Skip(1).First().Value;
            var todayDaysScore = .0035 * technicalEntries.First().Value;
            return 1 - todayDaysScore - minus4DaysScore - minus3DaysScore - minus2DaysScore - minus1DaysScore;
        }

        private async Task GetMacd(string symbol, StockMetrics metrics)
        {
            Console.WriteLine($"Updated {symbol}");

            string data = await _alphaClient.Execute(svc => svc.Custom()
            .Set("symbol", symbol)
            .Set("function", "MACD")
            .Set("interval", "daily")
            .Set("series_type", "close").GetRawDataAsync());

            JObject representation = JsonConvert.DeserializeObject<JObject>(data);

            List<(double MACD, double Signal, double Historical)> technicals = representation.Last.First().Select(r => (
                r.First()["MACD"].Value<double>(), 
                r.First()["MACD_Signal"].Value<double>(),
                r.First()["MACD_Hist"].Value<double>()
            )).Take(16).ToList();

            var macdScore = GetMacdScore(technicals);

            metrics.OverallScore = metrics.OverallScore + macdScore - metrics.MacdScore;
            metrics.MacdScore = macdScore;
            metrics.MacdUpdatedDate = DateTimeOffset.UtcNow;

            await _stockDataAccess.SaveMetrics(metrics);
        }

        private double GetMacdScore(List<(double MACD, double Signal, double Historical)> technicals)
        {
            var today = technicals.First();
            var last = technicals.Last();
            var mid = technicals[8];

            var totalMacd = (today.MACD - last.MACD) / 4;
            var macdVelocity1 = (today.MACD - mid.MACD) / 2;
            var macdVelocity2 = (mid.MACD -last.MACD) / 2;
            var macdAcceleration = (macdVelocity1 - macdVelocity2) / 2;

            var totalSignal = (today.Signal - last.Signal) / 4;
            var signalVelocity1 = (today.Signal - mid.Signal) / 2;
            var signalVelocity2 = (mid.Signal - last.Signal) / 2;
            var signalAcceleration = (signalVelocity1 - signalVelocity2) / 2;

            var historicalAvg = technicals.Average(x => x.Historical);

            var macdVelocity = .3 * totalMacd;
            var macdAccel = .15 * macdAcceleration;
            var signalVelocity = .12 * totalSignal;
            var signalAccel = .08 * signalAcceleration;
            var histAvg = historicalAvg * .13;
            var histCurrent = today.Historical * .22;

            return macdVelocity + macdAccel + signalVelocity + signalAccel + histAvg + histCurrent;
        }

        #endregion
    }
}