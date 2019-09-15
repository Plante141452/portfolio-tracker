using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlphaVantage.Net.Stocks.TimeSeries;
using PortfolioTracker.Origin.AlphaClient.Interfaces;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.DataAccess.Interfaces;

namespace PortfolioTracker.Origin.AlphaClient
{
    public class AlphaClientLogic : IAlphaClientLogic
    {
        private IAlphaClientWrapper _alphaClient;
        private readonly IStockDataAccess _stockData;

        public AlphaClientLogic(IAlphaClientWrapper alphaClient, IStockDataAccess stockData)
        {
            _alphaClient = alphaClient;
            _stockData = stockData;
        }

        public async Task<List<StockHistory>> GetHistory(List<string> symbols)
        {
            ConcurrentQueue<StockHistory> histories = new ConcurrentQueue<StockHistory>();

            await Task.WhenAll(symbols.Select(s => Task.Run(async () =>
            {
                var existingData = await _stockData.GetHistory(s);
                var lastRecordedClose = existingData?.History?.FirstOrDefault()?.ClosingDate;
                if (lastRecordedClose != null && DateTime.Now.Subtract(lastRecordedClose.Value).TotalDays < 7)
                    histories.Enqueue(existingData);
                else
                {
                    var data = await _alphaClient.Execute(svc => svc.RequestWeeklyTimeSeriesAsync(s, true));
                    var history = TransformSeries(data);

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
            var existingDataToMerge = existingData.History.Where(h => !updatedData.History.Any(uh => uh.ClosingDate.Equals(h.ClosingDate))).ToList();

            if (existingDataToMerge.Count == existingData.History.Count)
                throw new Exception("Cannot merge historical data as they do not overlap.");

            updatedData.History.AddRange(existingDataToMerge);
            updatedData.History = updatedData.History.OrderBy(h => h.ClosingDate).ToList();

            StockHistoryItem lastClosing = updatedData.History.First();
            foreach (var history in updatedData.History.Skip(1))
            {
                history.AdjustedClose = lastClosing.AdjustedClose * lastClosing.AdjustedPercentChanged;
                lastClosing = history;
            }

            return updatedData;
        }

        private StockHistory TransformSeries(StockTimeSeries series)
        {
            var history = series.DataPoints.ToList();
            return new StockHistory
            {
                Symbol = series.Symbol,
                History = history.Skip(1).Select((dp, i) => new StockHistoryItem
                {
                    ClosingDate = dp.Time,
                    Volume = dp.Volume,
                    AdjustedClose = dp.ClosingPrice,
                    AdjustedPercentChanged = i == 0 ? 0 : dp.ClosingPrice / history[i - 1].ClosingPrice
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
                if (DateTime.Now.Subtract(quote.UpdatedDate).TotalMinutes > 5)
                    quotesToRefresh.Add(quote);
                else
                {
                    if (symbols.Contains(quote.Symbol))
                        symbols.Remove(quote.Symbol);
                    quotesToReuse.Add(quote);
                }
            }

            var symbolsToRetrieve = quotesToRefresh.Select(q => q.Symbol).Union(symbols).Distinct();

            if (symbolsToRetrieve.Any())
            {
                var quotes = await _alphaClient.Execute(svc => svc.RequestBatchQuotesAsync(symbols.ToArray()));
                var mappedQuotes = quotes.Select(q => new Quote
                {
                    Symbol = q.Symbol,
                    Price = q.Price,
                    QuoteDate = q.Time,
                    UpdatedDate = DateTime.Now,
                    Volume = q.Volume ?? 0
                }).ToList();

                quotesToRefresh = mappedQuotes;
                await _stockData.SaveQuotes(quotesToRefresh);
            }

            return quotesToReuse.Union(quotesToRefresh).ToList();
        }
    }
}