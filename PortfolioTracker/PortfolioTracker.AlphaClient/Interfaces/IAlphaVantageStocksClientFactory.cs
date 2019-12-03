using AlphaVantage.Net.Stocks;

namespace PortfolioTracker.AlphaClient.Interfaces
{
    public interface IAlphaVantageStocksClientFactory
    {
        AlphaVantageStocksClient GetClient();
    }
}