using Newtonsoft.Json;

namespace SaudiPlus.Request.Branch
{
    public interface ICreateBranchDTO
    {
        string Name { get; set; }
        int BranchNumber { get; set; }
        int CityId { get; set; }
        string Address { get; set; }
        string PlaceId { get; set; }
        double Latitude { get; set; }
        double Longitude { get; set; }
    }

    public class BusinessCreateBranchDTO : ICreateBranchDTO
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("branchNumber")]
        public required int BranchNumber { get; set; }

        [JsonProperty("cityId")]
        public required int CityId { get; set; }

        [JsonProperty("address")]
        public required string Address { get; set; }

        [JsonProperty("latitude")]
        public required double Latitude { get; set; }

        [JsonProperty("longitude")]
        public required double Longitude { get; set; }

        [JsonProperty("placeId")]
        public required string PlaceId { get; set; }
    }

    public class AdminCreateBranchDTO : BusinessCreateBranchDTO
    {
        [JsonProperty("businessId")]
        public required Guid BusinessId { get; set; }
    }
}
