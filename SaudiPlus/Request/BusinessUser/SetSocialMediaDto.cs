using Newtonsoft.Json;

namespace SaudiPlus.Request.BusinessUser
{
    public class SetSocialMediaDto
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("page")]
        public string? Page { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }
    }
}
