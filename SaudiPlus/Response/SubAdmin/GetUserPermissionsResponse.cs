using Newtonsoft.Json;

namespace SaudiPlus.Response.SubAdmin
{
    public class GetUserPermissionsResponse : BaseResponse<GetUserPermissionsPayload>
    {
        public GetUserPermissionsResponse(
            int statusCode,
            string message,
            GetUserPermissionsPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetUserPermissionsPayload
    {
        [JsonProperty("permissions")]
        public required List<PermissionGroup> Permissions { get; set; }
    }

    public class PermissionGroup
    {
        [JsonProperty("group")]
        public required string Group { get; set; }

        [JsonProperty("items")]
        public required List<UserPermissionItem> Items { get; set; }
    }

    public class UserPermissionItem
    {
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }
    }
}
