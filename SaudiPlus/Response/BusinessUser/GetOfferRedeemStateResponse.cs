using Newtonsoft.Json;

namespace SaudiPlus.Response.BusinessUser
{
    public class GetOfferRedeemStateResponse : BaseResponse<OfferRedeemStatePayload>
    {
        public GetOfferRedeemStateResponse(
            int statusCode,
            string message,
            OfferRedeemStatePayload? payload
        )
            : base(statusCode, message, payload)
        {
        }
    }

    public class OfferRedeemStatePayload
    {
        [JsonProperty("stats")]
        public required List<OfferRedeemStat> Stats { get; set; }
    }

    public class OfferRedeemStat
    {
        [JsonProperty("year")]
        public int Year { get; set; }


        [JsonProperty("month")]
        public int Month { get; set; }


        [JsonProperty("totalRedeemCount")]
        public int TotalRedeemCount { get; set; }
    }
}
