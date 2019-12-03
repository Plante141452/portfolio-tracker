using System.Runtime.Serialization;

namespace PortfolioTracker.Models
{
    public enum MessageTypeEnum
    {
        [EnumMember]
        Information,

        [EnumMember]
        Warning,

        [EnumMember]
        Error
    }
}
