using Newtonsoft.Json;
using SaudiPlus.Enums;

namespace SaudiPlus.Request.Policy
{
    public class SetPolicyDTO
    {
        [JsonProperty("type")]
        public required string Type { get; set; }

        [JsonProperty("contentAr")]
        public required string ContentAr { get; set; }

        [JsonProperty("contentEn")]
        public required string ContentEn { get; set; }
    }
}
