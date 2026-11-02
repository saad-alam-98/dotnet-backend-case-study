using Newtonsoft.Json;

namespace SaudiPlus.Response.User
{
    public class GetPointsResponse : BaseResponse<GetUserPointsPayload>
    {
        public GetPointsResponse(
            int statusCode,
            string message,
            GetUserPointsPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetUserPointsPayload
    {
        [JsonProperty("totalPoints")]
        public required string TotalPoints { get; set; }
    }
}
