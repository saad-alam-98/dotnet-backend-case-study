using Newtonsoft.Json;

namespace SaudiPlus.Response.Report
{
    public class GetOfferUsageDetailsByMonthResponse : BaseResponse<OfferUsageDetailsByMonthPayload>
    {
        public GetOfferUsageDetailsByMonthResponse(
            int statusCode,
            string message,
            OfferUsageDetailsByMonthPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class OfferUsageDetailsByMonthPayload
    {
        [JsonProperty("totalOffers")]
        public required int TotalOffers;

        [JsonProperty("totalBusinessUsers")]
        public required int TotalBusinessUsers;

        [JsonProperty("totalUsage")]
        public required int TotalUsage;
    }
}
