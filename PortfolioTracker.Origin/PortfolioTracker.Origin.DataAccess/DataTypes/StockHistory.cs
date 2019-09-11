using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.DataAccess.DataTypes
{
    public class StockHistoryData
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Symbol { get; set; }
        public List<StockHistoryItem> History { get; set; }
    }
}