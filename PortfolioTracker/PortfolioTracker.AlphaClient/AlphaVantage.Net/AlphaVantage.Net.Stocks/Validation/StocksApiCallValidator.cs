using System.Collections.Generic;
using PortfolioTracker.AlphaClient.AlphaVantage.Net.AlphaVantage.Net.Core;
using PortfolioTracker.AlphaClient.AlphaVantage.Net.AlphaVantage.Net.Core.Validation;

namespace PortfolioTracker.AlphaClient.AlphaVantage.Net.AlphaVantage.Net.Stocks.Validation
{
    internal class StocksApiCallValidator : IApiCallValidator
    {
        public ValidationResult Validate(ApiFunction function, IDictionary<string, string> query)
        {
            throw new System.NotImplementedException();
        }
    }
}