using Newtonsoft.Json;

namespace SaudiPlus.Response.BusinessUser
{
    public class GetBusinessWebisteResponse : BaseResponse<GetBusinessWebistePayload>
    {
        public GetBusinessWebisteResponse(
            int statusCode,
            string message,
            GetBusinessWebistePayload? payload
        )
            : base(statusCode, message, payload)
        {
        }
    }

    public class GetBusinessWebistePayload
    {
        [JsonProperty("websiteURL")]
        public string? WebsiteURL { get; set; }
    }
}
