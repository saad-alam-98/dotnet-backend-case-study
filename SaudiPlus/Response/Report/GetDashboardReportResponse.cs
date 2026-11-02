using Newtonsoft.Json;

namespace SaudiPlus.Response.Report
{
    public class GetDashboardReportResponse : BaseResponse<GetDashboardReportPayload>
    {
        public GetDashboardReportResponse(
            int statusCode,
            string message,
            GetDashboardReportPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetDashboardReportPayload
    {
        [JsonProperty("downloads")]
        public required int Downloads;
        
        [JsonProperty("businessUsers")]
        public required int BusinessUsers;

        [JsonProperty("individualUsers")]
        public required int IndividualUsers { get; set; }
        
        [JsonProperty("branches")]
        public required int Branches { get; set; } 
        
        [JsonProperty("activeOffers")]
        public required int ActiveOffers { get; set; }
    }
}
