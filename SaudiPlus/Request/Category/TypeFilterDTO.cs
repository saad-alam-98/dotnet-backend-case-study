using Newtonsoft.Json;

namespace SaudiPlus.Request.Category
{
    public class TypeFilterDTO
    {
        [JsonProperty("type")]
        public required string Type { get; set; }

        [JsonProperty("categoryId")]
        public string? CategoryId { get; set; }

        [JsonProperty("pageNumber")]
        public int pageNumber { get; set; }

        [JsonProperty("pageSize")]
        public int pageSize { get; set; }

        [JsonProperty("searchKeyword")]
        public string? searchKeyword { get; set; }

        [JsonProperty("latitude")]
        public double? Latitude { get; set; }

        [JsonProperty("longitude")]
        public double? Longitude { get; set; }

        [JsonProperty("sortBySlug")]
        public string? SortBySlug { get; set; }

        [JsonProperty("distanceSlug")]
        public string? DistanceSlug { get; set; }

        [JsonProperty("discountSlug")]
        public string? DiscountSlug { get; set; }

        public TypeFilterDTO()
        {
            pageNumber = 1;
            pageSize = 10;
        }

        public TypeFilterDTO(int pageNumber, int pageSize, bool? isActive = null, string? searchKeyword = null)
        {
            this.pageNumber = pageNumber < 1 ? 1 : pageNumber;
            this.pageSize = pageSize <= 10 ? 10 : pageSize;
            this.searchKeyword = searchKeyword;
        }
    }
}
