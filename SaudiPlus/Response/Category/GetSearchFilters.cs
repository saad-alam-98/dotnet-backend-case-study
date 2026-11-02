using Newtonsoft.Json;

namespace SaudiPlus.Response.Category
{
    public class GetSearchFilters : BaseResponse<SearchFilterPayload>
    {
        public GetSearchFilters(
            int statusCode,
            string message,
            SearchFilterPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class SearchFilterPayload
    {
        [JsonProperty("filter")]
        public required List<FilterCategory> Filter;
    }

    public class FilterCategory
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("lookup")]
        public required string Lookup { get; set; }

        [JsonProperty("items")]
        public required List<FilterItem> Items { get; set; }
    }

    public class FilterItem
    {
        [JsonProperty("text")]
        public required string Text { get; set; }

        [JsonProperty("slug")]
        public required string Slug { get; set; } 
        
        [JsonProperty("imageUrl")]
        public string? ImageUrl { get; set; }
    }
}
