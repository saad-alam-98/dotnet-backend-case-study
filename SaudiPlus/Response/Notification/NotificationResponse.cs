using Newtonsoft.Json;
using SaudiPlus.Response.City;
using SaudiPlus.Response.Offer;

namespace SaudiPlus.Response.Notification
{
    public class GetNotificationResponse : BaseResponse<NotificationsPayload>
    {
        public GetNotificationResponse(
            int statusCode,
            string message,
            NotificationsPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

     public class GetSingleNotificationResponse : BaseResponse<NotificationsSinglePayload>
    {
        public GetSingleNotificationResponse(
            int statusCode,
            string message,
            NotificationsSinglePayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class NotificationsPayload
    {
        [JsonProperty("items")]
        public required List<BaseNotificationItem> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class NotificationsSinglePayload
    {
        [JsonProperty("items")]
        public required List<NotificationSingleItem> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }


    public class NotificationResponse<T> : BaseResponse<T>
    {
        public int TotalItems { get; set; }
        public bool HasNextPage { get; set; }

        public NotificationResponse(
            int statusCode,
            string message,
            T? payload,
            int totalItems = 0,
            bool hasNextPage = false
        ) : base(statusCode, message, payload)
        {
            TotalItems = totalItems;
            HasNextPage = hasNextPage;
        }
    }

    public class NotificationUserItem
    {
        [JsonProperty("userId")]
        public Guid UserId { get; set; }

        [JsonProperty("email")]
        public required string Email { get; set; }

        [JsonProperty("displayName")]
        public required string DisplayName { get; set; }

        [JsonProperty("avatarUrl")]
        public string? AvatarUrl { get; set; }
    }
    public class NotificationSingleItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
    public class BaseNotificationItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("titleAr")]
        public required string TitleAr { get; set; }

        [JsonProperty("titleEn")]
        public required string TitleEn { get; set; }

        [JsonProperty("messageAr")]
        public required string MessageAr { get; set; }

        [JsonProperty("messageEn")]
        public required string MessageEn { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class IndividualNotificationPayload : BaseNotificationItem
    {
        [JsonProperty("user")]
        public required NotificationUserItem User { get; set; }
    }
    public class GroupNotificationPayload : BaseNotificationItem
    {
        [JsonProperty("users")]
        public required NotificationUserItem[] Users { get; set; }
    }
    public class GeneralNotificationPayload : BaseNotificationItem { }
}