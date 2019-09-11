using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlphaVantage.Net.Stocks;
using PortfolioTracker.Origin.AlphaClient.Interfaces;

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
}