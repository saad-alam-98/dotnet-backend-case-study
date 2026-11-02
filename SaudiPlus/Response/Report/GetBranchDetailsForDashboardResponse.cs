using Newtonsoft.Json;

namespace SaudiPlus.Response.Report
{
    public class GetBranchDetailsForDashboardResponse : BaseResponse<GetBranchDetailsForDashboardPayload>
    {
        public GetBranchDetailsForDashboardResponse(
            int statusCode,
            string message,
            GetBranchDetailsForDashboardPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetBranchDetailsForDashboardPayload
    {
        [JsonProperty("items")]
        public required List<BranchDetailsForDashboardItem> Items;
    }

    public class BranchDetailsForDashboardItem
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("placeId")]
        public required string PlaceId { get; set; }

        [JsonProperty("logoPath")]
        public required string LogoPath { get; set; }
    }
}
