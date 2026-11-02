using Newtonsoft.Json;

namespace SaudiPlus.Response.Partner
{
    public class GetPartnersResponse : BaseResponse<GetPartnersPayload>
    {
        public GetPartnersResponse(
                int statusCode,
                string message,
                GetPartnersPayload? payload
            ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetPartnersPayload
    {
        [JsonProperty("items")]
        public required List<BasePartnerInfoPayload> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }
}
