
using Newtonsoft.Json;

namespace SaudiPlus.Response.Account
{
    public class LoginResponse : BaseResponse<LoginResponsePayload>
    {
        public LoginResponse(
            int statusCode,
            string message,
            LoginResponsePayload? payload
        )
            : base(statusCode, message, payload)
        {
        }
    }

    public class LoginResponsePayload
    {
        public required string accessToken { get; set; }
        public required string refreshToken { get; set; }
        public DateTime expiry { get; set; }
        public required string role { get; set; }
        public required object? data { get; set; }
    }

    public class LoginData
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("email")]
        public required string Email { get; set; }

        [JsonProperty("displayName")]
        public required string DisplayName { get; set; }
    }
}
