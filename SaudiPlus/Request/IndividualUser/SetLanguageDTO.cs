using Newtonsoft.Json;

namespace SaudiPlus.Request.IndividualUser
{
    public class SetLanguageDTO
    {
        [JsonProperty("language")]
        public required string Language { get; set; }
    }
}
