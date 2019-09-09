using MongoDB.Driver;
using System;

namespace PortfolioTracker.Origin.DataAccess
{
    public class MongoClientWrapper
    {
        private readonly IConnectionStringProvider _connectionStringProvider;
        private readonly Lazy<MongoClient> _mongoClientLazy;

        public MongoClient MongoClient => _mongoClientLazy.Value;

        public MongoClientWrapper(IConnectionStringProvider connectionStringProvider)
        {
            _connectionStringProvider = connectionStringProvider;
            _mongoClientLazy = new Lazy<MongoClient>(() => new MongoClient(_connectionStringProvider.ConnectionString));
        }
    }
}
