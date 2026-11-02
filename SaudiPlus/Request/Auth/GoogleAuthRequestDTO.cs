using Newtonsoft.Json;

namespace SaudiPlus.Request.Auth
{
    public class GoogleAuthRequestDTO
    {
        [JsonProperty("idToken")]
        public required string IdToken { get; set; }
    }

    public class AppleAuthRequestDTO
    {
        [JsonProperty("idToken")]
        public required string IdToken { get; set; }

        [JsonProperty("fullName")]
        public string? FullName { get; set; }
    }
}
