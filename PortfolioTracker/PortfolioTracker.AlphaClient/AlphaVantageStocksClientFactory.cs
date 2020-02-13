using PortfolioTracker.AlphaClient.AlphaVantage.Net.AlphaVantage.Net.Stocks;
using PortfolioTracker.AlphaClient.Interfaces;
using SharpCompress;

namespace PortfolioTracker.AlphaClient
{
    public class AlphaVantageStocksClientFactory : IAlphaVantageStocksClientFactory
    {
        private readonly Lazy<AlphaVantageStocksClient> _clientLazy;

        public AlphaVantageStocksClientFactory()
            : this(new ApiKeyProvider())
        {
        }

        public AlphaVantageStocksClientFactory(IApiKeyProvider apiKeyProvider)
        {
            _clientLazy = new Lazy<AlphaVantageStocksClient>(() => new AlphaVantageStocksClient(apiKeyProvider.AlphaVantageKey));
        }

        public AlphaVantageStocksClient GetClient()
        {
            return _clientLazy.Value;
        }
    }
}