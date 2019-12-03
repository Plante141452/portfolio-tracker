using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PortfolioTracker.Models;

namespace PortfolioTracker.DataAccess.DataTypes
{
    public class StockHistoryData : StockHistory
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}