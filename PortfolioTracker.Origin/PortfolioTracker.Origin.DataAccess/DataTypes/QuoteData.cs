using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.DataAccess.DataTypes
{
    public class QuoteData : Quote
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}