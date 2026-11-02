using Newtonsoft.Json;

namespace SaudiPlus.Response.Status
{
    public class GetUserStatusListResponse : BaseResponse<UserStatusCollection>
    {
        public GetUserStatusListResponse(
            int statusCode,
            string message,
            UserStatusCollection? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class UserStatusCollection
    {
        [JsonProperty("items")]
        public required List<UserStatusPayload> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class UserStatusPayload
    {
        [JsonProperty("id")]
        public required int Id { get; set; }

        [JsonProperty("title")]
        public required string Title { get; set; }

        [JsonProperty("mediaUrl")]
        public required string MediaUrl { get; set; }

        [JsonProperty("mediaType")]
        public required string MediaType { get; set; }

        [JsonProperty("isActive")]
        public required bool IsActive { get; set; }

        [JsonProperty("isClicked")]
        public required bool IsClicked { get; set; }

        [JsonProperty("isViewed")]
        public required bool IsViewed { get; set; }

        [JsonProperty("duration")]
        public required double Duration { get; set; }

        [JsonProperty("viewDuration")]
        public required double ViewDuration { get; set; }

        [JsonProperty("Description")]
        public string? Description { get; set; }

        [JsonProperty("linkUrl")]
        public string? LinkUrl { get; set; }

        [JsonProperty("businessId")]
        public Guid? BusinessId { get; set; }

        [JsonProperty("offerId")]
        public int? OfferId { get; set; }

        [JsonProperty("creationTime")]
        public DateTime CreationTime { get; set; }
    }
}
