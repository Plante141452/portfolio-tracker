using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PortfolioTracker.Origin.IEX.Tests
{
    [TestFixture]
    public class IEXClientTests
    {
        private IEXClient _client = new IEXClient();

        [Test]
        public async Task Test()
        {
            var test = await _client.GetQuote("MSFT");
            Assert.IsNotNull(test);
        }
    }
}
