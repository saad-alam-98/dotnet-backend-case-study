using Newtonsoft.Json;
using SaudiPlus.Enums;

namespace SaudiPlus.Response.Policy
{
    public class GetPolicyResponse : BaseResponse<BasePolicyPayload>
    {
        public GetPolicyResponse(
            int statusCode,
            string message,
            BasePolicyPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class BasePolicyPayload
    {
        [JsonProperty("id")]
        public required int Id { get; set; }

        [JsonProperty("type")]
        public required PolicyType Type { get; set; }
    }

    public class PolicyDetailed : BasePolicyPayload
    {
        [JsonProperty("contentAr")]
        public required string ContentAr { get; set; }

        [JsonProperty("contentEn")]
        public required string ContentEn { get; set; }
    }

    public class PolicyLocalized : BasePolicyPayload
    {
        [JsonProperty("content")]
        public required string Content { get; set; }

        public void SetPolicyContentLocalized(string content)
        {
            Content = content;
        }
    }
}
