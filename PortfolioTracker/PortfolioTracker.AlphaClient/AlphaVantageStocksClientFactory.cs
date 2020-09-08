using PortfolioTracker.AlphaClient.Interfaces;
using System;

namespace PortfolioTracker.AlphaClient
{
    public class AlphaVantageStocksClientFactory : IAlphaVantageStocksClientFactory
    {
        private readonly Lazy<ThreeFourteen.AlphaVantage.AlphaVantage> _clientLazy;

        public AlphaVantageStocksClientFactory()
            : this(new ApiKeyProvider())
        {
        }

        public AlphaVantageStocksClientFactory(IApiKeyProvider apiKeyProvider)
        {
            var apiKey = apiKeyProvider.AlphaVantageKey;
            _clientLazy = new Lazy<ThreeFourteen.AlphaVantage.AlphaVantage>(() => new ThreeFourteen.AlphaVantage.AlphaVantage(apiKey));
        }

        public ThreeFourteen.AlphaVantage.AlphaVantage GetClient()
        {
            return _clientLazy.Value;
        }
    }
}