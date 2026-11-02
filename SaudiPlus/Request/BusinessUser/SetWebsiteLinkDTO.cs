using Newtonsoft.Json;

namespace SaudiPlus.Request.BusinessUser
{
    public class SetWebsiteLinkDTO
    {
        [JsonProperty("websiteURL")]
        public string? WebsiteURL { get; set; }
    }
}
