using System.Runtime.Serialization;

namespace PortfolioTracker.Models.Enums
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