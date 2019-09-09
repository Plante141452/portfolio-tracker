using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Linq;

namespace PortfolioTracker.Origin.DataAccess.Tests
{
    public class MongoClientWrapperTests
    {
        private MongoClientWrapper _wrapper;

        [SetUp]
        public void Setup()
        {
            _wrapper = new MongoClientWrapper(new ConnectionStringProvider());
        }

        [Test]
        public async Task Test()
        {
            var db = _wrapper.MongoClient.GetDatabase("stock_data");

            await db.CreateCollectionAsync("stocks_test");

            var collection = db.GetCollection<BsonDocument>("stocks_test");

            await collection.InsertOneAsync(new BsonDocument
            {
                { "name", "MongoDB" },
                { "type", "Database" },
                { "count", 1 },
                { "info", new BsonDocument
                    {
                        { "x", 203 },
                        { "y", 102 }
                    }}
            });

            var filter = Builders<BsonDocument>.Filter.Eq("name", "MongoDB");
            var cursor = await collection.FindAsync<BsonDocument>(filter);
            var document = await cursor.FirstOrDefaultAsync();
        }
    }
}
