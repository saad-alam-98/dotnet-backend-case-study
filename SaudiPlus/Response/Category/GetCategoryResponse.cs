using Newtonsoft.Json;

namespace SaudiPlus.Response.Category
{
    public class GetCategoryResponse : BaseResponse<BaseCategoryInfo>
    {
        public GetCategoryResponse(
            int statusCode,
            string message,
            BaseCategoryInfo? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetCategoriesResponse : BaseResponse<GetCategoryPayload>
    {
        public GetCategoriesResponse(
            int statusCode,
            string message,
            GetCategoryPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }


    public class GetCategoryPayload
    {
        [JsonProperty("items")]
        public required List<BaseCategoryInfo> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }

    }

    public abstract class BaseCategoryInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("imagePath")]
        public required string ImagePath;
    }

    public class CategoryInfoDetailed : BaseCategoryInfo
    {
        [JsonProperty("nameAr")]
        public string NameAr { get; set; } = null!;

        [JsonProperty("nameEn")]
        public string NameEn { get; set; } = null!;

        [JsonProperty("isActive")]
        public required bool IsActive { get; set; }
    }

    public class CategoryInfoLocalized : BaseCategoryInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; } = null!;
    }
}
