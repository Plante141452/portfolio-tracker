using System;

namespace PortfolioTracker.AlphaClient.AlphaVantage.Net.AlphaVantage.Net.Core.Exceptions
{
    public class AlphaVantageException : Exception
    {
        public AlphaVantageException(string message) : base(message)
        {
        }

        public AlphaVantageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}