using Newtonsoft.Json;

namespace SaudiPlus.Response.BusinessUser
{
    public class GetBusinessByLocationResponse : BaseResponse<BusinessByLocationPayload>
    {
        public GetBusinessByLocationResponse(
            int statusCode,
            string message,
            BusinessByLocationPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class BusinessByLocationPayload
    {
        [JsonProperty("items")]
        public required List<BusinessLocationItem> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class BusinessLocationItem
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("description")]
        public required string Description { get; set; }

        [JsonProperty("email")]
        public required string Email { get; set; }

        [JsonProperty("logoPath")]
        public required string LogoPath { get; set; }

        [JsonProperty("branchInfo")]
        public required BranchLocationItem BranchInfo { get; set; }
    }

    public class BranchLocationItem
    {
        [JsonProperty("id")]
        public required int Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("branchNumber")]
        public required int BranchNumber { get; set; }

        [JsonProperty("kmAway")]
        public double KmAway { get; set; }

        [JsonProperty("placeId")]
        public required string PlaceId { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }
}
