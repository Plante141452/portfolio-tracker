using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PortfolioTracker.Origin.AlphaClient.Interfaces;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.DataAccess.Interfaces;
using PortfolioTracker.Origin.IEX;

namespace PortfolioTracker.Origin.AlphaClient
{
    public class IexClientLogic : IAlphaClientLogic
    {
        private readonly IIEXClient _iexClient;
        private readonly IStockDataAccess _stockData;

        public IexClientLogic(IStockDataAccess stockData, IIEXClient iexClient)
        {
            _stockData = stockData;
            _iexClient = iexClient;
        }

        public async Task<List<StockHistory>> GetHistory(List<string> symbols)
        {
            ConcurrentQueue<StockHistory> histories = new ConcurrentQueue<StockHistory>();

            await Task.WhenAll(symbols.Select(s => Task.Run(async () =>
            {
                var existingData = await _stockData.GetHistory(s);
                var lastRecordedClose = existingData?.History?.Max(h => h.ClosingDate);
                if (lastRecordedClose != null && DateTimeOffset.UtcNow.Subtract(lastRecordedClose.Value).TotalDays < 8)
                    histories.Enqueue(existingData);
                else
                {
                    var data = await _iexClient.GetHistory(s, TimeCadenceEnum.Weekly);
                    // var data = await _alphaClient.Execute(svc => svc.RequestWeeklyTimeSeriesAsync(s, true));
                    var history = TransformSeries(s, data);

                    if (existingData != null)
                        history = MergeHistories(history, existingData);

                    histories.Enqueue(history);
                    await _stockData.SaveHistory(history);
                }
            })));

            return histories.ToList();
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
                history.AdjustedClose = lastClosing.AdjustedClose * lastClosing.AdjustedPercentChanged;
                lastClosing = history;
            }

            return updatedData;
        }

        private StockHistory TransformSeries(string symbol, List<IEXHistoryContract> series)
        {
            return new StockHistory
            {
                Symbol = symbol,
                History = series.Select((dp, i) => new StockHistoryItem
                {
                    ClosingDate = dp.date,
                    Volume = dp.volume,
                    AdjustedClose = dp.close,
                    AdjustedPercentChanged = i == 0 ? 0 : dp.close / series[i - 1].close
                }).ToList()
            };
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
            var existingQuotes = await _stockData.GetQuotes();

            List<Quote> quotesToReuse = new List<Quote>();
            List<Quote> quotesToRefresh = new List<Quote>();

            foreach (var quote in existingQuotes)
            {
                if (DateTimeOffset.UtcNow.Subtract(quote.UpdatedDate).TotalMinutes > 5)
                    quotesToRefresh.Add(quote);
                else
                {
                    if (symbols.Contains(quote.Symbol))
                        symbols.Remove(quote.Symbol);
                    quotesToReuse.Add(quote);
                }
            }

            var symbolsToRetrieve = quotesToRefresh.Select(q => q.Symbol).Union(symbols).Distinct().ToList();

            if (symbolsToRetrieve.Any())
            {
                ConcurrentQueue<Quote> quotes = new ConcurrentQueue<Quote>();
                await Task.WhenAll(symbolsToRetrieve.Select(symbol => Task.Run(async () =>
                {
                    var quote = await _iexClient.GetQuote(symbol);
                    quotes.Enqueue(new Quote
                    {
                        Symbol = quote.symbol,
                        Price = quote.latestPrice,
                        QuoteDate = quote.latestUpdate,
                        UpdatedDate = DateTimeOffset.UtcNow
                    });
                })));

                quotesToRefresh = quotes.ToList();
                await _stockData.SaveQuotes(quotesToRefresh);
            }

            return quotesToReuse.Union(quotesToRefresh).ToList();
        }
    }
}