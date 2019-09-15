using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PortfolioTracker.Origin.DataAccess.DataTypes
{
    public class QuoteData
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public DateTime QuoteDate { get; set; }
        public long Volume { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}