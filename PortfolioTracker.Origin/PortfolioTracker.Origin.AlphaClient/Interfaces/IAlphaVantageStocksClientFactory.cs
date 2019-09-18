using AlphaVantage.Net.Stocks;

namespace PortfolioTracker.Origin.AlphaClient.Interfaces
{
    public interface IAlphaVantageStocksClientFactory
    {
        AlphaVantageStocksClient GetClient();
    }
}