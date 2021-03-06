﻿using System;
using MongoDB.Driver;
using PortfolioTracker.Origin.DataAccess.Interfaces;

namespace PortfolioTracker.Origin.DataAccess
{
    public class MongoClientWrapper : IMongoClientWrapper
    {
        private readonly Lazy<MongoClient> _mongoClientLazy;

        private const string StockData = "STOCK_DATA";
        private readonly Lazy<IMongoDatabase> _stockDatabaseLazy;
        public IMongoDatabase StockDatabase => _stockDatabaseLazy.Value;

        private MongoClient MongoClient => _mongoClientLazy.Value;

        public MongoClientWrapper(IConnectionStringProvider connectionStringProvider)
        {
            _mongoClientLazy = new Lazy<MongoClient>(() => new MongoClient(connectionStringProvider.ConnectionString));
            _stockDatabaseLazy = new Lazy<IMongoDatabase>(() => MongoClient.GetDatabase(StockData));
        }
    }
}