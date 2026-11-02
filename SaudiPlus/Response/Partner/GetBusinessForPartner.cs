using Newtonsoft.Json;

namespace SaudiPlus.Response.Partner
{
    public class GetBusinessForPartner : BaseResponse<GetBusinessForPartnerPayload>
    {
        public GetBusinessForPartner(
                int statusCode,
                string message,
                GetBusinessForPartnerPayload? payload
            ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetBusinessForPartnerPayload
    {
        public required List<GetBusinessForPartnerItem> items;
    }

    public class GetBusinessForPartnerItem
    {
        [JsonProperty("id")]
        public required Guid Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }
    }
}
