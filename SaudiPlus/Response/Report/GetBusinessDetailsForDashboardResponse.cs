using Newtonsoft.Json;
using SaudiPlus.Response.Contract;

namespace SaudiPlus.Response.Report
{
    public class GetBusinessDetailsForDashboardResponse : BaseResponse<GetBusinessDetailsForDashboardPayload>
    {
        public GetBusinessDetailsForDashboardResponse(
            int statusCode,
            string message,
            GetBusinessDetailsForDashboardPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetBusinessDetailsForDashboardPayload
    {
        [JsonProperty("items")]
        public required List<BusinessDetailsForDashboardItem> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class BusinessDetailsForDashboardItem
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("offerCount")]
        public required int OfferCount { get; set; }

        [JsonProperty("phoneNumber")]
        public required string PhoneNumber { get; set; }

        [JsonProperty("contract")]
        public required GetContractPayload? Contract { get; set; }
    }
}
