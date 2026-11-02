using Newtonsoft.Json;

namespace SaudiPlus.Request.Notification
{
    public abstract class BaseNotificationRequest
    {
        [JsonProperty("titleEn")]
        public string? TitleEn { get; set; }

        [JsonProperty("titleAr")]
        public string? TitleAr { get; set; }

        [JsonProperty("messageEn")]
        public required string MessageEn { get; set; }

        [JsonProperty("messageAr")]
        public required string MessageAr { get; set; }
    }
    public class IndividualNotificationRequestDTO : BaseNotificationRequest
    {
        [JsonProperty("email")]
        public required string Email { get; set; }
    }
    public class GroupUsersNotificationRequestDTO : BaseNotificationRequest
    {
        [JsonProperty("emails")]
        public required string[] Emails { get; set; }
    }
    public class GeneralNotificationRequestDTO : BaseNotificationRequest {}
}
