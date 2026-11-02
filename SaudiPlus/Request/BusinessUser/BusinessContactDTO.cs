using Newtonsoft.Json;

namespace SaudiPlus.Request.BusinessUser
{
    public class BusinessContactDTO
    {
        [JsonProperty("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonProperty("mobileNumber")]
        public string? MobileNumber { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }
    }
}
