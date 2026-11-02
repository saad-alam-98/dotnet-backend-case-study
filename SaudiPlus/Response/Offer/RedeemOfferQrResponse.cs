using Newtonsoft.Json;

namespace SaudiPlus.Response.Offer
{
    public class RedeemOfferQrResponse : BaseResponse<RedeemOfferQrPayload>
    {
        public RedeemOfferQrResponse(
            int statusCode,
            string message,
            RedeemOfferQrPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class RedeemOfferQrPayload
    {
        [JsonProperty("code")]
        public required string Code { get; set; }

        [JsonProperty("expiryTime")]
        public required DateTime ExpiryTime { get; set; }
    }
}
