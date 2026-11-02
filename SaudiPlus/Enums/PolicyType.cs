using System.Runtime.Serialization;

namespace SaudiPlus.Enums
{
    public enum PolicyType
    {
        [EnumMember(Value = "termsAndConditions")]
        TermsAndConditions = 1,

        [EnumMember(Value = "privacyPolicy")]
        PrivacyPolicy = 2,

        [EnumMember(Value = "aboutApp")]
        AboutApp = 3,

        [EnumMember(Value = "howToUse")]
        HowToUse = 4
    }
}
