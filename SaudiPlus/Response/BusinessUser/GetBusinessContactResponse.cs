using Newtonsoft.Json;

namespace SaudiPlus.Response.BusinessUser
{
    public class GetBusinessContactResponse : BaseResponse<GetBusinessContactPayload>
    {
        public GetBusinessContactResponse(
            int statusCode,
            string message,
            GetBusinessContactPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetBusinessContactPayload
    {
        [JsonProperty("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonProperty("mobileNumber")]
        public string? MobileNumber { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("facebookUrl")]
        public string? FacebookUrl { get; set; }

        [JsonProperty("facebookPage")]
        public string? FacebookPage { get; set; }

        [JsonProperty("instagramUrl")]
        public string? InstagramUrl { get; set; }

        [JsonProperty("instagramPage")]
        public string? InstagramPage { get; set; }

        [JsonProperty("snapchatUrl")]
        public string? SnapchatUrl { get; set; }

        [JsonProperty("snapchatPage")]
        public string? SnapchatPage { get; set; }

        [JsonProperty("twitterUrl")]
        public string? TwitterUrl { get; set; }

        [JsonProperty("twitterPage")]
        public string? TwitterPage { get; set; }
    }
}
