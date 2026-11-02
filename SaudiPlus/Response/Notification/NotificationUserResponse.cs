using Newtonsoft.Json;

namespace SaudiPlus.Response.Notification
{
    public class NotificationUserResponse : BaseResponse<NotificationUsersPayload>
    {
        public NotificationUserResponse(
            int statusCode,
            string message,
            NotificationUsersPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class NotificationUsersPayload
    {
        [JsonProperty("items")]
        public required List<NotificationUserItem> Items;
    }
}
