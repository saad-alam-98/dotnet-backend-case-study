using Newtonsoft.Json;
using SaudiPlus.Response.City;
using SaudiPlus.Response.BusinessUser;

namespace SaudiPlus.Response.Branch
{
    public class GetAllBranchResponse : BaseResponse<GetAllBranchesPayload>
    {
        public GetAllBranchResponse(
            int statusCode,
            string message,
            GetAllBranchesPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetBranchResponse : BaseResponse<BranchPayloadItem>
    {
        public GetBranchResponse(
            int statusCode,
            string message,
            BranchPayloadItem? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetBranchesResponse : BaseResponse<GetBranchesPayload>
    {
        public GetBranchesResponse(
            int statusCode,
            string message,
            GetBranchesPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetAllBranchesPayload
    {
        [JsonProperty("items")]
        public required List<BranchPayloadItem> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class GetBranchesPayload
    {
        public required List<BranchPayloadItem> items;
    }

    public class BranchPayloadItem
    {
        [JsonProperty("id")]
        public required int Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("branchNumber")]
        public required int BranchNumber { get; set; }

        [JsonProperty("address")]
        public required string Address { get; set; }

        [JsonProperty("qrCodePath")]
        public required string QrCodePath { get; set; }

        [JsonProperty("business")]
        public required BusinessInfoLocalized Business { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("placeId")]
        public required string PlaceId { get; set; }

        [JsonProperty("city")]
        public required CityInfoLocalized City { get; set; }

        public void SetBusinessNameLocalized(string name)
        {
            if (Business == null)
                return;

            Business.Name = name;
        }

        public void SetCityNameLocalized(string name)
        {
            if (City == null)
                return;

            City.Name = name;
        }
    }
}
