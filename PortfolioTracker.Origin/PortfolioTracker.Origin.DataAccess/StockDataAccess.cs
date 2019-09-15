using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.DataAccess.DataTypes;
using PortfolioTracker.Origin.DataAccess.Interfaces;

namespace PortfolioTracker.Origin.DataAccess
{
    public class StockDataAccess : IStockDataAccess
    {
        private readonly IMongoClientWrapper _mongoWrapper;

        public StockDataAccess(IMongoClientWrapper mongoWrapper)
        {
            _mongoWrapper = mongoWrapper;
        }

        private const string QuoteData = "quote_data";
        private const string StockHistoryData = "stock_history_data";

        public async Task<List<Quote>> GetQuotes()
        {
            var collection = _mongoWrapper.StockDatabase.GetCollection<QuoteData>(QuoteData);
            FilterDefinition<QuoteData> filter = Builders<QuoteData>.Filter.Empty;

            var data = await collection.FindAsync(filter);
            var list = await data.ToListAsync();

            return list.Select(q => new Quote
            {
                Symbol = q.Symbol,
                Price = q.Price,
                QuoteDate = q.QuoteDate,
                UpdatedDate = q.UpdatedDate,
                Volume = q.Volume
            }).ToList();
        }

        public async Task SaveQuotes(List<Quote> quotes)
        {
            var collection = _mongoWrapper.StockDatabase.GetCollection<QuoteData>(QuoteData);

            var symbols = quotes.Select(q => q.Symbol).ToList();
            FilterDefinition<QuoteData> filter = Builders<QuoteData>.Filter.Where(s => symbols.Contains(s.Symbol));
            var filteredData = await collection.FindAsync(filter);

            var existing = await filteredData.ToListAsync();

            var create = quotes.Where(q => existing.All(eq => eq.Symbol != q.Symbol)).ToList();
            var update = quotes.Select(q => new { QuoteData = q, existing.Find(eq => eq.Symbol == q.Symbol)?.Id }).Where(q => q.Id != null).ToList();

            if (create.Any())
            {
                var docs = create.Select(q => new QuoteData
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Price = q.Price,
                    Symbol = q.Symbol,
                    QuoteDate = q.QuoteDate,
                    UpdatedDate = q.UpdatedDate,
                    Volume = q.Volume
                }).ToList();
                await collection.InsertManyAsync(docs);
            }

            if (update.Any())
            {
                await Task.WhenAll(update.Select(u => Task.Run(async () =>
                {
                    var updateFilter = Builders<QuoteData>.Filter.Eq(q => q.Id, u.Id);
                    await collection.ReplaceOneAsync(updateFilter, new QuoteData
                    {
                        Id = u.Id,
                        Price = u.QuoteData.Price,
                        Symbol = u.QuoteData.Symbol,
                        QuoteDate = u.QuoteData.QuoteDate,
                        UpdatedDate = u.QuoteData.UpdatedDate,
                        Volume = u.QuoteData.Volume
                    });
                })));
            }
        }

        public async Task<StockHistory> GetHistory(string symbol)
        {
            var collection = _mongoWrapper.StockDatabase.GetCollection<StockHistoryData>(StockHistoryData);
            FilterDefinition<StockHistoryData> filter = Builders<StockHistoryData>.Filter.Eq(s => s.Symbol, symbol);
            var filteredData = await collection.FindAsync(filter);
            var data = await filteredData.FirstOrDefaultAsync();
            if (data == null)
                return null;

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