using System.Runtime.Serialization;

namespace SaudiPlus.Enums
{
    public enum TicketStatus
    {
        [EnumMember(Value = "new")]
        New = 0,

        [EnumMember(Value = "open")]
        Open = 1,

        [EnumMember(Value = "close")]
        Close = 2
    }
}
