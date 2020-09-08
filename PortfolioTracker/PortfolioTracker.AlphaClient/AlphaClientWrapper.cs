using PortfolioTracker.AlphaClient.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PortfolioTracker.AlphaClient
{
    public class AlphaClientWrapper : IAlphaClientWrapper
    {
        private readonly IAlphaVantageStocksClientFactory _clientFactory;

        public AlphaClientWrapper()
            : this(new AlphaVantageStocksClientFactory())
        {
        }

        public AlphaClientWrapper(IAlphaVantageStocksClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<T> Execute<T>(Func<ThreeFourteen.AlphaVantage.AlphaVantage, Task<T>> exec)
        {
            var client = _clientFactory.GetClient();

            try
            {
                return await exec(client);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}