using Newtonsoft.Json;

namespace SaudiPlus.Response.Report
{
    public class GetBranchReportResponse : BaseResponse<BranchReportPayload>
    {
        public GetBranchReportResponse(
            int statusCode,
            string message,
            BranchReportPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class BranchReportPayload
    {
        [JsonProperty("items")]
        public required List<BusinessReportItem> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class BusinessReportItem
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("branches")]
        public required List<BranchReportItem> Branches { get; set; }

        [JsonProperty("totalUsage")]
        public required double TotalUsage { get; set; }

        [JsonProperty("sales")]
        public required double Sales { get; set; }
    }

    public class BranchReportItem
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("totalUsage")]
        public required double TotalUsage { get; set; }

        [JsonProperty("sales")]
        public required double Sales { get; set; }
    }
}
