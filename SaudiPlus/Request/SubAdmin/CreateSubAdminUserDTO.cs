using Newtonsoft.Json;

namespace SaudiPlus.Request.SubAdmin
{
    public class CreateSubAdminUserDTO
    {
        [JsonProperty("username")]
        public required string Username { get; set; }

        [JsonProperty("email")]
        public required string Email { get; set; }

        [JsonProperty("password")]
        public required string Password { get; set; }

        [JsonProperty("permissions")]
        public required List<string> Permissions { get; set; }
    }
}
