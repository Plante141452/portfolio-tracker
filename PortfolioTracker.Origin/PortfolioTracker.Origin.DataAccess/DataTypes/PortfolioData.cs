using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.DataAccess.DataTypes
{
    public class PortfolioData : Portfolio
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public override string Id { get; set; }
    }
}