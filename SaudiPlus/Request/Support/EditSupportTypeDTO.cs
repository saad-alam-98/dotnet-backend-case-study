using Newtonsoft.Json;

namespace SaudiPlus.Request.Support
{
    public class EditSupportTypeDTO
    {
        [JsonProperty("id")]
        public required int Id { get; set; }

        [JsonProperty("nameAr")]
        public string? NameAr { get; set; }

        [JsonProperty("nameEn")]
        public string? NameEn { get; set; }

        [JsonProperty("isActive")]
        public bool? IsActive { get; set; }
    }
}
