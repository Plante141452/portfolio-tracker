using System;
using PortfolioTracker.AlphaClient.AlphaVantage.Net.AlphaVantage.Net.Core.Exceptions;

namespace PortfolioTracker.AlphaClient.AlphaVantage.Net.AlphaVantage.Net.Stocks.Parsing.Exceptions
{
    public class StocksParsingException : AlphaVantageException
    {
        public StocksParsingException(string message) : base(message)
        {
        }

        public StocksParsingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}