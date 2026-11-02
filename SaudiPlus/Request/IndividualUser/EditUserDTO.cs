using Newtonsoft.Json;
using SaudiPlus.Enums;

namespace SaudiPlus.Request.IndividualUser
{
    public class EditUserDTO
    {
        [JsonProperty("displayName")]
        public string? DisplayName { get; set; }

        [JsonProperty("isActive")]
        public bool? IsActive { get; set; }

        [JsonProperty("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("dateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [JsonProperty("gender")]
        public Gender? Gender { get; set; }

        [JsonProperty("cityId")]
        public int? CityId { get; set; }
    }
}
