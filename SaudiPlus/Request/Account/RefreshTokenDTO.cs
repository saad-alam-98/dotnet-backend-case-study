using Newtonsoft.Json;

namespace SaudiPlus.Request.Account
{
    public class RefreshTokenDTO
    {
        [JsonProperty("email")]
        public required string Email { get; set; }

        [JsonProperty("refreshToken")]
        public required string RefreshToken { get; set; }
    }
}
