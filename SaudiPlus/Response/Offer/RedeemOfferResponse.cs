using Newtonsoft.Json;

namespace SaudiPlus.Response.Offer
{
    public class RedeemOfferResponse : BaseResponse<RedeemOfferPayload>
    {
        public RedeemOfferResponse(
            int statusCode,
            string message,
            RedeemOfferPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class RedeemOfferPayload
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("redeemCode")]
        public required string RedeemCode { get; set; }

        [JsonProperty("title")]
        public required string Title { get; set; }

        [JsonProperty("description")]
        public required string Description { get; set; }

        [JsonProperty("points")]
        public required int Points { get; set; }

        [JsonProperty("source")]
        public required string Source { get; set; }

        [JsonProperty("creationTime")]
        public DateTime CreationTime { get; set; }
    }
}
