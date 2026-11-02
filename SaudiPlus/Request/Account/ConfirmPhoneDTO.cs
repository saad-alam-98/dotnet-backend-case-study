using Newtonsoft.Json;

namespace SaudiPlus.Request.Account
{
    public class ConfirmPhoneDTO
    {
        [JsonProperty("otp")]
        public required string Otp { get; set; }
    }
}
