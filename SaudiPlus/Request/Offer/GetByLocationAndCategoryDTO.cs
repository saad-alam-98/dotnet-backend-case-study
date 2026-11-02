using Newtonsoft.Json;

namespace SaudiPlus.Request.Offer
{
    public class GetByLocationAndCategoryDTO
    {
        [JsonProperty("latitude")]
        public double? Latitude { get; set; }

        [JsonProperty("longitude")]
        public double? Longitude { get; set; }

        [JsonProperty("categoryId")]
        public string? CategoryId { get; set; }

        [JsonProperty("sortBySlug")]
        public string? SortBySlug { get; set; }

        [JsonProperty("distanceSlug")]
        public string? DistanceSlug { get; set; }

        [JsonProperty("discountSlug")]
        public string? DiscountSlug { get; set; }
    }
}
