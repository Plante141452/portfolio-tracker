using System.Threading.Tasks;
using NUnit.Framework;
using PortfolioTracker.Origin.DataAccess.Interfaces;

namespace PortfolioTracker.Origin.DataAccess.Tests
{
    [TestFixture]
    public class StockDataAccessTests
    {
        private IStockDataAccess _stockDataAccess;

        [SetUp]
        public void Setup()
        {
            _stockDataAccess = new StockDataAccess(new MongoClientWrapper(new ConnectionStringProvider()));
        }

        [Test]
        public async Task Test()
        {
            var quotes = await _stockDataAccess.GetQuotes();
        }
    }
}