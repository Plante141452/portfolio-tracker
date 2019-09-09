namespace PortfolioTracker.Origin.DataAccess
{
    public class ConnectionStringProvider : IConnectionStringProvider
    {
        public string ConnectionString => "mongodb+srv://sa:<password>@portfolio-tracker-origin-tt20v.mongodb.net/test?retryWrites=true&w=majority";
    }
}
