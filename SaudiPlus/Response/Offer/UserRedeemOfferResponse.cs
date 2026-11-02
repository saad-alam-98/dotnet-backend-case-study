
using Newtonsoft.Json;

namespace SaudiPlus.Response.Offer
{
    public class UserRedeemOfferResponse : BaseResponse<UserRedeemOfferPayload>
    {
        public UserRedeemOfferResponse(
            int statusCode,
            string message,
            UserRedeemOfferPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class UserRedeemOfferPayload
    {
        [JsonProperty("totalPoints")]
        public required int TotalPoints { get; set; }

        [JsonProperty("offers")]
        public required List<RedeemOfferPayload> Offers { get; set; }
    }
}
