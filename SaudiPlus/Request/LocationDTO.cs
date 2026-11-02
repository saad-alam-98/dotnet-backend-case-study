using Newtonsoft.Json;

namespace SaudiPlus.Request
{
    public class LocationDTO
    {
        [JsonProperty("latitude")]
        public double? Latitude { get; set; }

        [JsonProperty("longitude")]
        public double? Longitude { get; set; }
    }
}
