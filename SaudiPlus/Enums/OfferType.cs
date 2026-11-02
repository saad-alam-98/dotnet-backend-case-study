using System.Runtime.Serialization;

namespace SaudiPlus.Enums
{
    public enum OfferType
    {
        [EnumMember(Value = "invoice")]
        Invoice = 0,

        [EnumMember(Value = "percentage")]
        Percentage = 1
    }

    public enum OfferTags
    {
        ComingSoon,
        Active,
        LimitedTime,
        Hot,
        ExpiringSoon,
        Expired
    }
}
