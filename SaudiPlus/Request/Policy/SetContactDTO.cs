using Newtonsoft.Json;

namespace SaudiPlus.Request.Policy
{
    public class SetContactDTO
    {
        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("phone")]
        public string? Phone { get; set; }

        [JsonProperty("whatsApp")]
        public string? WhatsApp { get; set; }
    }
}
