using Newtonsoft.Json;

namespace SaudiPlus.Request.SubAdmin
{
    public class EditSubAdminUserDTO
    {
        [JsonProperty("id")]
        public required Guid Id { get; set; }

        [JsonProperty("username")]
        public string? Username { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("password")]
        public string? Password { get; set; }

        [JsonProperty("isActive")]
        public bool? IsActive { get; set; }

        [JsonProperty("permissions")]
        public required List<string> Permissions { get; set; }
    }
}
