using Newtonsoft.Json;

namespace SaudiPlus.Request.Branch
{
    public class EditBranchDTO : BaseEntityDTO<int>
    {
        [JsonProperty("name")]
        public required string? Name { get; set; }

        [JsonProperty("isActive")]
        public bool? IsActive { get; set; }

        [JsonProperty("cityId")]
        public int? CityId { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("latitude")]
        public double? Latitude { get; set; }

        [JsonProperty("longitude")]
        public double? Longitude { get; set; }

        [JsonProperty("placeId")]
        public string? PlaceId { get; set; }

        [JsonProperty("qrCodePath")]
        public string? QrCodePath { get; set; }
    }
}
