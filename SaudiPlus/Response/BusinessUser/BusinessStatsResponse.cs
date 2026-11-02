using Newtonsoft.Json;

namespace SaudiPlus.Response.BusinessUser
{
    public class BusinessStatsResponse : BaseResponse<BusinessStatsPayload>
    {
        public BusinessStatsResponse(
            int statusCode,
            string message,
            BusinessStatsPayload? payload
        )
            : base(statusCode, message, payload)
        {
        }
    }

    public class BusinessStatsPayload
    {
        [JsonProperty("totalRedeemCount")]
        public int TotalRedeemCount { get; set; }

        [JsonProperty("totalSalesAmount")]
        public double TotalSalesAmount { get; set; }

        [JsonProperty("totalBranches")]
        public double TotalBranches { get; set; }

        [JsonProperty("totalOffers")]
        public double TotalOffers { get; set; }
    }
}
