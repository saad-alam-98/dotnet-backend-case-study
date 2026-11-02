
using Newtonsoft.Json;

namespace SaudiPlus.Request.City
{
    public class AddCityDTO
    {
        [JsonProperty("nameAr")]
        public required string NameAr { get; set; }

        [JsonProperty("nameEn")]
        public required string NameEn { get; set; }

        [JsonProperty("isActive")]
        public required bool IsActive { get; set; }
    }
}
