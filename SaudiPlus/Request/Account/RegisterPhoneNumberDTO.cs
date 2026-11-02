using Newtonsoft.Json;

namespace SaudiPlus.Request.Account
{
    public class RegisterPhoneNumberDTO
    {
        [JsonProperty("phoneNumber")]
        public required string PhoneNumber { get; set; }
    }
}
