using Newtonsoft.Json;

namespace SaudiPlus.Response.Status
{
    public class GetStatusListResponse : BaseResponse<StatusCollection>
    {
        public GetStatusListResponse(
            int statusCode,
            string message,
            StatusCollection? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class StatusCollection
    {
        [JsonProperty("items")]
        public required List<GetStatusPayload> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }
}
