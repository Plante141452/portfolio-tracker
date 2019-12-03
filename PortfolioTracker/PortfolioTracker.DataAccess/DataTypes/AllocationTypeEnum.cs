using System.Runtime.Serialization;

namespace PortfolioTracker.DataAccess.DataTypes
{
    public enum AllocationTypeEnum
    {
        [EnumMember(Value = "Percentage")]
        Percentage,

        [EnumMember(Value = "CashAmount")]
        CashAmount,

        [EnumMember(Value = "StockAmount")]
        StockAmount
    }
}