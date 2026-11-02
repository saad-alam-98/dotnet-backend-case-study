using Newtonsoft.Json;

namespace SaudiPlus.Response.SubAdmin
{
    public class GetSubAdminsResponse : BaseResponse<GetSubAdminsPayload>
    {
        public GetSubAdminsResponse(
            int statusCode,
            string message,
            GetSubAdminsPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetSubAdminsPayload
    {

        [JsonProperty("items")]
        public required List<GetSubAdminPayload> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }
}
