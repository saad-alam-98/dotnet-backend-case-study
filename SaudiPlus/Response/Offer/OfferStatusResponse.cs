using Newtonsoft.Json;

namespace SaudiPlus.Response.Offer
{
    public class OfferStatusResponse : BaseResponse<OfferStatusPayload>
    {
        public OfferStatusResponse(
            int statusCode,
            string message,
            OfferStatusPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class OfferStatusPayload
    {
        [JsonProperty("totalOffers")]
        public int TotalOffers { get; set; }

        [JsonProperty("activeOffers")]
        public int ActiveOffers { get; set; }

        [JsonProperty("inActiveOffers")]
        public int InActiveOffers { get; set; }
    }
}
