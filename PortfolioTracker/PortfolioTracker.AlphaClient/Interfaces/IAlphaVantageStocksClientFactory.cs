
namespace PortfolioTracker.AlphaClient.Interfaces
{
    public interface IAlphaVantageStocksClientFactory
    {
        ThreeFourteen.AlphaVantage.AlphaVantage GetClient();
    }
}