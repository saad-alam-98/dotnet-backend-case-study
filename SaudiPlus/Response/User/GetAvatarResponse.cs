using Newtonsoft.Json;

namespace SaudiPlus.Response.User
{
    public class GetAvatarResponse : BaseResponse<GetAvatarPayload>
    {
        public GetAvatarResponse(
            int statusCode,
            string message,
            GetAvatarPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetAvatarPayload
    {
        [JsonProperty("avatarUrl")]
        public required string AvatarUrl { get; set; }
    }
}
