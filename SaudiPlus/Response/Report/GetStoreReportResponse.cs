using Newtonsoft.Json;

namespace SaudiPlus.Response.Report
{
    public class GetStoreReportResponse : BaseResponse<StoreReportPayload>
    {
        public GetStoreReportResponse(
            int statusCode,
            string message,
            StoreReportPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class StoreReportPayload
    {
        [JsonProperty("items")]
        public required List<StoreReportItem> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class StoreReportItem
    {
        [JsonProperty("name")]
        public required string Name { get; set; }
        
        [JsonProperty("totalOffers")]
        public required int TotalOffers { get; set; }
        
        [JsonProperty("totalUsage")]
        public required int TotalUsage { get; set; }

        [JsonProperty("sales")]
        public required double Sales { get; set; }
    }
}
