using MongoDB.Driver;

namespace PortfolioTracker.Origin.DataAccess
{
    public interface IMongoClientWrapper
    {
        IMongoDatabase StockDatabase { get; }
    }
}