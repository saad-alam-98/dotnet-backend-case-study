using Newtonsoft.Json;

namespace SaudiPlus.Response.Branch
{
    public class GetBranchesPaginatedResponse : BaseResponse<GetBranchesPaginatedPayload>
    {
        public GetBranchesPaginatedResponse(
            int statusCode,
            string message,
            GetBranchesPaginatedPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetBranchesPaginatedPayload
    {
        [JsonProperty("items")]
        public required List<BranchPayloadItem> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }
}
