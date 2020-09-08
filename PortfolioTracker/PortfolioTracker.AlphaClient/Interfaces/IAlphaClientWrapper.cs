using System;
using System.Threading.Tasks;

namespace PortfolioTracker.AlphaClient.Interfaces
{
    public interface IAlphaClientWrapper
    {
        Task<T> Execute<T>(Func<ThreeFourteen.AlphaVantage.AlphaVantage, Task<T>> task);
    }
}