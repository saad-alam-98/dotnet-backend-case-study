using Newtonsoft.Json;

namespace SaudiPlus.Request.Support
{
    public class AddSupportTypeDTO
    {
        [JsonProperty("nameAr")]
        public required string NameAr { get; set; }

        [JsonProperty("nameEn")]
        public required string NameEn { get; set; }
    }
}
