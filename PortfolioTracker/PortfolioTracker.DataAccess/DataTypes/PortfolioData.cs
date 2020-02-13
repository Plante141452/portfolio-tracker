using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PortfolioTracker.Models;

namespace PortfolioTracker.DataAccess.DataTypes
{
    public class PortfolioData : Portfolio
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public override string Id { get; set; }
    }
}