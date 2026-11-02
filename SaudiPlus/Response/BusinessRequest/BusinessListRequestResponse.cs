using Newtonsoft.Json;

namespace SaudiPlus.Response.BusinessRequest
{
    public class BusinessListRequestResponse : BaseResponse<BusinessListPayload>
    {
        public BusinessListRequestResponse(
            int statusCode,
            string message,
            BusinessListPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class BusinessListPayload
    {
        [JsonProperty("items")]
        public required List<BusinessRequestPayload> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }
}
