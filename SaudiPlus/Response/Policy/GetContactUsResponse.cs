using Newtonsoft.Json;

namespace SaudiPlus.Response.Policy
{
    public class GetContactUsResponse : BaseResponse<ContactUsPayload>
    {
        public GetContactUsResponse(
            int statusCode,
            string message,
            ContactUsPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class ContactUsPayload
    {
        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("phone")]
        public string? Phone { get; set; }

        [JsonProperty("whatsApp")]
        public string? WhatsApp { get; set; }

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
