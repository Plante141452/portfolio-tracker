using AlphaVantage.Net.Stocks;
using PortfolioTracker.AlphaClient.Interfaces;

namespace PortfolioTracker.AlphaClient
{
    public class AlphaVantageStocksClientFactory : IAlphaVantageStocksClientFactory
    {
        private readonly IApiKeyProvider _apiKeyProvider;

        public AlphaVantageStocksClientFactory(IApiKeyProvider apiKeyProvider)
        {
            _apiKeyProvider = apiKeyProvider;
        }

        public AlphaVantageStocksClient GetClient()
        {
            return new AlphaVantageStocksClient(_apiKeyProvider.AlphaVantageKey);
        }
    }
}