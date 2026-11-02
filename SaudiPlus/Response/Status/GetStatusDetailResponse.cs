using Newtonsoft.Json;

namespace SaudiPlus.Response.Status
{
    public class GetStatusDetailResponse : BaseResponse<GetStatusPayload>
    {
        public GetStatusDetailResponse(
            int statusCode,
            string message,
            GetStatusPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetStatusPayload
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

        [JsonProperty("duration")]
        public required double Duration { get; set; }

        [JsonProperty("Description")]
        public string? Description { get; set; }

        [JsonProperty("linkUrl")]
        public string? LinkUrl { get; set; }

        [JsonProperty("businessId")]
        public Guid? BusinessId { get; set; }

        [JsonProperty("offerId")]
        public int? OfferId { get; set; }

        [JsonProperty("totalInteractionTime")]
        public required int TotalInteractionTime { get; set; }

        [JsonProperty("totalClicks")]
        public required int TotalClicks { get; set; }

        [JsonProperty("totalViews")]
        public required int TotalViews { get; set; }

        [JsonProperty("creationTime")]
        public required DateTime CreationTime { get; set; }
    }
}
