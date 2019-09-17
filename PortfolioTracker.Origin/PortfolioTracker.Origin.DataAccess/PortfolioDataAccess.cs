using MongoDB.Bson;
using MongoDB.Driver;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.DataAccess.DataTypes;
using PortfolioTracker.Origin.DataAccess.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioTracker.Origin.DataAccess
{
    public class PortfolioDataAccess
    {
        private readonly IMongoClientWrapper _mongoWrapper;

        public PortfolioDataAccess(IMongoClientWrapper mongoWrapper)
        {
            _mongoWrapper = mongoWrapper;
        }

        private const string PortfolioData = "portfolio_data";

        public async Task<List<Portfolio>> GetPortfolio(string portfolioId)
        {
            var collection = _mongoWrapper.StockDatabase.GetCollection<PortfolioData>(PortfolioData);
            FilterDefinition<PortfolioData> filter = Builders<PortfolioData>.Filter.Empty;

            var data = await collection.FindAsync(filter);
            var list = await data.ToListAsync();

            return list.Select(p => new Portfolio
            {
                Id = p.Id,
                CashOnHand = p.CashOnHand,
                Categories = p.Categories,
                Name = p.Name,
                Stocks = p.Stocks
            }).ToList();
        }

        public async Task SavePortfolios(List<Portfolio> portfolios)
        {
            var collection = _mongoWrapper.StockDatabase.GetCollection<PortfolioData>(PortfolioData);

            var create = portfolios.Where(p => string.IsNullOrWhiteSpace(p.Id)).ToList();
            var update = portfolios.Where(p => !string.IsNullOrWhiteSpace(p.Id)).ToList();

            if (create.Any())
            {
                var docs = create.Select(p => new PortfolioData
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    CashOnHand = p.CashOnHand,
                    Categories = p.Categories,
                    Name = p.Name,
                    Stocks = p.Stocks
                }).ToList();

                await collection.InsertManyAsync(docs);
            }

            if (update.Any())
            {
                await Task.WhenAll(update.Select(p => Task.Run(async () =>
                {
                    var updateFilter = Builders<PortfolioData>.Filter.Eq(x => x.Id, p.Id);
                    await collection.ReplaceOneAsync(updateFilter, new PortfolioData
                    {
                        Id = p.Id,
                        CashOnHand = p.CashOnHand,
                        Categories = p.Categories,
                        Name = p.Name,
                        Stocks = p.Stocks
                    });
                })));
            }
        }

        //public async Task<StockHistory> GetHistory(string symbol)
        //{
        //    var collection = _mongoWrapper.StockDatabase.GetCollection<StockHistoryData>(StockHistoryData);
        //    FilterDefinition<StockHistoryData> filter = Builders<StockHistoryData>.Filter.Eq(s => s.Symbol, symbol);
        //    var filteredData = await collection.FindAsync(filter);
        //    var data = await filteredData.FirstOrDefaultAsync();
        //    if (data == null)
        //        return null;
        //
        //    return new StockHistory
        //    {
        //        Symbol = data.Symbol,
        //        History = data.History.OrderByDescending(h => h.ClosingDate).ToList()
        //    };
        //}
        //
        //public async Task SaveHistory(StockHistory history)
        //{
        //    var collection = _mongoWrapper.StockDatabase.GetCollection<StockHistoryData>(StockHistoryData);
        //
        //    FilterDefinition<StockHistoryData> filter = Builders<StockHistoryData>.Filter.Eq(s => s.Symbol, history.Symbol);
        //    var filteredData = await collection.FindAsync(filter);
        //
        //    var existing = filteredData.FirstOrDefault();
        //
        //    if (existing == null)
        //    {
        //        var data = new StockHistoryData
        //        {
        //            Id = ObjectId.GenerateNewId().ToString(),
        //            Symbol = history.Symbol,
        //            History = history.History.OrderByDescending(h => h.ClosingDate).ToList()
        //        };
        //
        //        await collection.InsertOneAsync(data);
        //    }
        //    else
        //    {
        //        UpdateDefinition<StockHistoryData> update = Builders<StockHistoryData>.Update.Set(s => s.History, history.History);
        //        await collection.UpdateOneAsync(filter, update);
        //    }
        //}
    }
}