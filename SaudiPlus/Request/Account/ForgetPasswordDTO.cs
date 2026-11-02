using Newtonsoft.Json;

namespace SaudiPlus.Request.Account
{
    public class ForgetPasswordDTO
    {
        [JsonProperty("email")]
        public required string Email { get; set; }
    }
}
