using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.DataAccess.DataTypes;

namespace PortfolioTracker.Origin.DataAccess
{
    public class StockDataAccess : IStockDataAccess
    {
        private readonly IMongoClientWrapper _mongoWrapper;

        public StockDataAccess(IMongoClientWrapper mongoWrapper)
        {
            _mongoWrapper = mongoWrapper;
        }

        private const string StockHistoryData = "stock_history_data";

        public async Task<StockHistory> GetHistory(string symbol)
        {
            var collection = _mongoWrapper.StockDatabase.GetCollection<StockHistoryData>(StockHistoryData);
            FilterDefinition<StockHistoryData> filter = Builders<StockHistoryData>.Filter.Eq(s => s.Symbol, symbol);
            var filteredData = await collection.FindAsync(filter);
            var data = filteredData.FirstOrDefault();

            return new StockHistory
            {
                Symbol = data.Symbol,
                History = data.History
            };
        }

        public async Task SaveHistory(StockHistory history)
        {
            var collection = _mongoWrapper.StockDatabase.GetCollection<StockHistoryData>(StockHistoryData);

            FilterDefinition<StockHistoryData> filter = Builders<StockHistoryData>.Filter.Eq(s => s.Symbol, history.Symbol);
            var filteredData = await collection.FindAsync(filter);

            var existing = filteredData.FirstOrDefault();

            if (existing == null)
            {
                var data = new StockHistoryData
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Symbol = history.Symbol,
                    History = history.History
                };

                await collection.InsertOneAsync(data);
            }
            else
            {
                UpdateDefinition<StockHistoryData> update = Builders<StockHistoryData>.Update.Set(s => s.History, history.History);
                await collection.UpdateOneAsync(filter, update);
            }
        }
    }
}