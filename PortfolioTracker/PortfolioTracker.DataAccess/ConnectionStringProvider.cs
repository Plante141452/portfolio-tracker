using PortfolioTracker.DataAccess.Interfaces;

namespace PortfolioTracker.DataAccess
{
    public class ConnectionStringProvider : IConnectionStringProvider
    {
        public string ConnectionString => "mongodb+srv://sa:ewq123@portfolio-tracker-origin-tt20v.mongodb.net/test?retryWrites=true&w=majority";
    }
}
