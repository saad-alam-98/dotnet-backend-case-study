using Newtonsoft.Json;

namespace SaudiPlus.Request.BusinessUser
{
    public class SetBusinessAppLinksDTO
    {
        [JsonProperty("androidAppUrl")]
        public string? AndroidAppUrl { get; set; }

        [JsonProperty("iOSAppUrl")]
        public string? iOSAppUrl { get; set; }
    }
}
