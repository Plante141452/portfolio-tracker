using MongoDB.Driver;

namespace PortfolioTracker.DataAccess.Interfaces
{
    public interface IMongoClientWrapper
    {
        IMongoDatabase StockDatabase { get; }
    }
}