using Newtonsoft.Json;

namespace SaudiPlus.Request.IndividualUser
{
    public class RegisterUserDTO
    {
        [JsonProperty("email")]
        public required string Email { get; set; }

        [JsonProperty("password")]
        public required string Password { get; set; }

        [JsonProperty("confirmPassword")]
        public required string ConfirmPassword { get; set; }

        [JsonProperty("displayName")]
        public required string DisplayName { get; set; }

        [JsonProperty("cityId")]
        public int? CityId { get; set; }
    }
}
