using Newtonsoft.Json;

namespace SaudiPlus.Response.BusinessUser
{
    public class GetBusinessSalesResponse : BaseResponse<GetBusinessSalesPayload>
    {
        public GetBusinessSalesResponse(
            int statusCode,
            string message,
            GetBusinessSalesPayload? payload
        )
            : base(statusCode, message, payload)
        {
        }
    }


    public class GetBusinessSalesPayload
    {
        [JsonProperty("stats")]
        public required List<BusinessSalesState> Stats { get; set; }
    }

    public class BusinessSalesState
    {
        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("totalSalesAmount")]
        public double TotalSalesAmount { get; set; }
    }
}
