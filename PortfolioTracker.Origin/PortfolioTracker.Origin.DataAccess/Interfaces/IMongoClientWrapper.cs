using MongoDB.Driver;

namespace PortfolioTracker.Origin.DataAccess.Interfaces
{
    public interface IMongoClientWrapper
    {
        IMongoDatabase StockDatabase { get; }
    }
}