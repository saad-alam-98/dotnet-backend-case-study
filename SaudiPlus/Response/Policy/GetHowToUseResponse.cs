using Newtonsoft.Json;

namespace SaudiPlus.Response.Policy
{
    public class GetHowToUseResponse : BaseResponse<HowToUsePayload>
    {
        public GetHowToUseResponse(
               int statusCode,
               string message,
               HowToUsePayload? payload
           ) : base(statusCode, message, payload)
        {
        }
    }

    public class HowToUsePayload
    {
        [JsonProperty("data")]
        public required string Data { get; set; }
    }
}
