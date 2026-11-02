using Newtonsoft.Json;

namespace SaudiPlus.Request.IndividualUser
{
    public class GuestLoginDTO
    {
        [JsonProperty("deviceIdentifier")]
        public required string DeviceIdentifier { get; set; }

        [JsonProperty("cityId")]
        public int CityId { get; set; }
    }
}
