using Newtonsoft.Json;
using SaudiPlus.Response.IndividualUser;

namespace SaudiPlus.Response.User
{
    public class GetUsersResponse : BaseResponse<GetUserPayload>
    {
        public GetUsersResponse(
            int statusCode,
            string message,
            GetUserPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetUserPayload
    {
        [JsonProperty("items")]
        public required List<IndividualUserData> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class GetUserResponse : BaseResponse<IndividualUserData>
    {
        public GetUserResponse(
            int statusCode,
            string message,
            IndividualUserData? payload
        ) : base(statusCode, message, payload)
        {
        }
    }
}
