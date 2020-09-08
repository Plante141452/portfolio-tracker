using PortfolioTracker.DataAccess;
using PortfolioTracker.DataAccess.Interfaces;
using PortfolioTracker.IexClient;
using PortfolioTracker.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioTracker.Client
{
    public class StockApiWrapper : IStockApiWrapper
    {
        private readonly IIexClient _iexClient;
        private readonly IQueueWrapper _alphaQueue;
        private readonly IStockDataAccess _stockData;

        public StockApiWrapper()
            : this(new QueueWrapper("alpha-vantage-events"), new IexClient.IexClient(), new StockDataAccess())
        {
        }

        public StockApiWrapper(IQueueWrapper alphaQueue, IIexClient iexClient, IStockDataAccess stockData)
        {
            _alphaQueue = alphaQueue;
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
                if (lastRecordedClose == null || !(DateTimeOffset.UtcNow.Subtract(lastRecordedClose.Value).TotalDays < 8))
                    await _alphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = s });
                histories.Enqueue(existingData);
            })));

            return histories.ToList();
        }

        public Task<List<dynamic>> GetMetrics(List<string> quoteSymbols)
        {
            throw new NotImplementedException();
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

            List<Quote> result = new List<Quote>();
            List<string> symbolsToRetrieve = symbols.Where(s => existingQuotes.All(q => q.Symbol != s)).ToList();

            foreach (var quote in existingQuotes)
            {
                if (symbols.Contains(quote.Symbol) && DateTimeOffset.UtcNow.Subtract(quote.UpdatedDate).TotalMinutes > 15)
                    symbolsToRetrieve.Add(quote.Symbol);
                else
                    result.Add(quote);
            }

            if (symbolsToRetrieve.Any())
            {
                ConcurrentQueue<Quote> quotes = new ConcurrentQueue<Quote>();
                await Task.WhenAll(symbolsToRetrieve.Select(symbol => Task.Run(async () =>
                {
                    var quote = await _iexClient.GetQuote(symbol);
                    if (quote != null)
                        quotes.Enqueue(new Quote
                        {
                            Symbol = quote.symbol,
                            Price = quote.latestPrice,
                            QuoteDate = quote.latestUpdate,
                            UpdatedDate = DateTimeOffset.UtcNow
                        });
                })));

                var refreshedQuotes = quotes.ToList();
                result.AddRange(refreshedQuotes);
                await _stockData.SaveQuotes(refreshedQuotes);
            }

            return result;
        }
    }
}