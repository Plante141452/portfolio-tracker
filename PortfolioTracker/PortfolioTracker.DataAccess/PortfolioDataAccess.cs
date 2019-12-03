using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using PortfolioTracker.DataAccess.DataTypes;
using PortfolioTracker.DataAccess.Interfaces;

namespace PortfolioTracker.Origin.DataAccess
{
    public class PortfolioDataAccess : IPortfolioDataAccess
    {
        private readonly IMongoClientWrapper _mongoWrapper;

        public PortfolioDataAccess(IMongoClientWrapper mongoWrapper)
        {
            _mongoWrapper = mongoWrapper;
        }

        private const string PortfolioData = "portfolio_data";

        public async Task<Portfolio> GetPortfolio(string portfolioId)
        {
            var collection = _mongoWrapper.StockDatabase.GetCollection<PortfolioData>(PortfolioData);
            FilterDefinition<PortfolioData> filter = Builders<PortfolioData>.Filter.Eq(p => p.Id, portfolioId);

            var data = await collection.FindAsync(filter);
            var pData = await data.FirstOrDefaultAsync();

            return new Portfolio
            {
                Id = pData.Id,
                CashOnHand = pData.CashOnHand,
                Categories = pData.Categories,
                Name = pData.Name,
                Stocks = pData.Stocks
            };
        }

        public async Task<List<Portfolio>> SavePortfolios(List<Portfolio> portfolios)
        {
            ConcurrentQueue<Portfolio> results = new ConcurrentQueue<Portfolio>();

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

                foreach (var doc in docs)
                    results.Enqueue(doc);
            }

            if (update.Any())
            {
                await Task.WhenAll(update.Select(p => Task.Run(async () =>
                {
                    var updateFilter = Builders<PortfolioData>.Filter.Eq(x => x.Id, p.Id);

                    var replacement = new PortfolioData
                    {
                        Id = p.Id,
                        CashOnHand = p.CashOnHand,
                        Categories = p.Categories,
                        Name = p.Name,
                        Stocks = p.Stocks
                    };

                    await collection.ReplaceOneAsync(updateFilter, replacement);
                    results.Enqueue(replacement);
                })));
            }

            return results.ToList();
        }
    }
}