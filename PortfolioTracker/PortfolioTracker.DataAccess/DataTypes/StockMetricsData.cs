using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PortfolioTracker.Models;

namespace PortfolioTracker.DataAccess.DataTypes
{
    public class StockMetricsData : StockMetrics
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}