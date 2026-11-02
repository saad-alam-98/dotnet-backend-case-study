using Newtonsoft.Json;

namespace SaudiPlus.Response.Offer
{
    public class GetBusinessForOffersResponse : BaseResponse<GetBusinessForOffersPayload>
    {
        public GetBusinessForOffersResponse(
            int statusCode,
            string message,
            GetBusinessForOffersPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetBusinessForOffersPayload
    {
        [JsonProperty("items")]
        public required List<BusinessForOffersItem> Items;
    }

    public class BusinessForOffersItem
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }
    }
}
