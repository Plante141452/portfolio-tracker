using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlphaVantage.Net.Stocks;
using AlphaVantage.Net.Stocks.TimeSeries;
using PortfolioTracker.Origin.AlphaClient.Interfaces;
using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.AlphaClient
{
    public class AlphaClient : IAlphaClient
    {
        private readonly Lazy<AlphaVantageStocksClient> _clientLazy;
        private readonly IApiKeyProvider _apiKeyProvider;

        private AlphaVantageStocksClient Client => _clientLazy.Value;

        public AlphaClient(IApiKeyProvider apiKeyProvider)
        {
            _apiKeyProvider = apiKeyProvider;
            _clientLazy = new Lazy<AlphaVantageStocksClient>(() => new AlphaVantageStocksClient(_apiKeyProvider.AlphaVantageKey));
        }

        public async Task<List<StockHistory>> GetHistory(List<string> symbols)
        {
            ConcurrentQueue<StockTimeSeries> stockSeries = new ConcurrentQueue<StockTimeSeries>();

            await Task.WhenAll(symbols.Select(s => Task.Run(async () =>
            {
                var data = await Client.RequestWeeklyTimeSeriesAsync(s, true);
                stockSeries.Enqueue(data);
            })));
            //foreach(var s in symbols)
            //{
            //    var data = await Client.RequestWeeklyTimeSeriesAsync(s, true);
            //    stockSeries.Enqueue(data);
            //}

            return stockSeries.Select(series => new
            {
                series.Symbol,
                History = series.DataPoints.ToList()
            }).Select(series => new StockHistory
            {
                Symbol = series.Symbol,
                History = series.History.Select((dp, i) => new StockHistoryItem
                {
                    ClosingDate = dp.Time,
                    Volume = dp.Volume,
                    AdjustedClose = dp.ClosingPrice,
                    AdjustedPercentChanged = i == 0 ? 0 : dp.ClosingPrice / series.History[i - 1].ClosingPrice
                }).ToList()
            }).ToList();
        }

        public async Task<List<PortfolioHistoryPeriod>> GetPortfolioHistory(List<string> symbols)
        {
            var stockHistories = await GetHistory(symbols);

            var periods = stockHistories.SelectMany(h => h.History).Select(h => h.ClosingDate.Date).Distinct().ToList();

            return periods.Select(p => new PortfolioHistoryPeriod
            {
                ClosingDate = p,
                Stocks = stockHistories.Select(s => new StockHistoricalPeriod
                {
                    Symbol = s.Symbol,
                    PeriodData = s.History.Find(h => h.ClosingDate.Date.Equals(p))
                }).Where(s => s.PeriodData != null).ToList()
            }).Where(p => p.Stocks.Count == symbols.Count).OrderBy(p => p.ClosingDate).ToList();
        }

        public async Task<List<Quote>> GetQuotes(List<string> symbols)
        {
            var quotes = await Client.RequestBatchQuotesAsync(symbols.ToArray());
            return quotes.Select(q => new Quote
            {
                Symbol = q.Symbol,
                Price = q.Price,
                Time = q.Time,
                Volume = q.Volume ?? 0
            }).ToList();
        }
    }

    public class StockHistoricalPeriod
    {
        public string Symbol { get; set; }
        public StockHistoryItem PeriodData { get; set; }
    }

    public class PortfolioHistoryPeriod
    {
        public DateTime ClosingDate { get; set; }
        public List<StockHistoricalPeriod> Stocks { get; set; }
    }
}