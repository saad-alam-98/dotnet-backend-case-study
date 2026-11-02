using Newtonsoft.Json;

namespace SaudiPlus.Response.SubAdmin
{
    public class GetSubAdminResponse : BaseResponse<GetSubAdminPayload>
    {
        public GetSubAdminResponse(
            int statusCode,
            string message,
            GetSubAdminPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetSubAdminPayload
    {

        [JsonProperty("id")]
        public required Guid Id { get; set; }

        [JsonProperty("email")]
        public required string Email { get; set; }

        [JsonProperty("isActive")]
        public required string IsActive { get; set; }

        [JsonProperty("username")]
        public required string Username { get; set; }

        [JsonProperty("creationTime")]
        public DateTime CreationTime { get; set; }

        [JsonProperty("permissions")]
        public required string[] Permissions { get; set; }
    }
}
