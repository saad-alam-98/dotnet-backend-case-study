using Newtonsoft.Json;

namespace SaudiPlus.Response.BusinessUser
{
    public class GetBusinessAppResponse : BaseResponse<GetBusinessAppPayload>
    {
        public GetBusinessAppResponse(
            int statusCode,
            string message,
            GetBusinessAppPayload? payload
        )
            : base(statusCode, message, payload)
        {
        }
    }

    public class GetBusinessAppPayload
    {
        [JsonProperty("androidAppUrl")]
        public string? AndroidAppUrl { get; set; }

        [JsonProperty("iOSAppUrl")]
        public string? iOSAppUrl { get; set; }
    }
}
