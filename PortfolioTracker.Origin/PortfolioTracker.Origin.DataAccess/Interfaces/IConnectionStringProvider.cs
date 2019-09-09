namespace PortfolioTracker.Origin.DataAccess
{
    public interface IConnectionStringProvider
    {
        string ConnectionString { get; }
    }
}