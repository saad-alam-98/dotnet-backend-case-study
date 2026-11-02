using Newtonsoft.Json;
using SaudiPlus.Response.City;
using SaudiPlus.Response.Category;
using SaudiPlus.Response.BusinessUser;

namespace SaudiPlus.Response.Banner
{
    public class GetBannersResponse : BaseResponse<GetBannerPayload>
    {
        public GetBannersResponse(
            int statusCode,
            string message,
            GetBannerPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetBannerPayload
    {
        [JsonProperty("items")]
        public required List<BannerPayloadItem> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class BannerPayloadItem
    {
        [JsonProperty("bannerId")]
        public int BannerId { get; set; }

        [JsonProperty("title")]
        public required string Title;

        [JsonProperty("imagePath")]
        public required string ImagePath;

        [JsonProperty("isActive")]
        public required bool IsActive;

        [JsonProperty("business")]
        public BusinessInfoLocalized? Business;

        [JsonProperty("category")]
        public CategoryInfoLocalized? Category;

        [JsonProperty("city")]
        public required CityInfoLocalized City;

        public void SetBusinessNameLocalized(string name)
        {
            if (Business == null)
                return;

            Business.Name = name;
        }

        public void SetCategoryNameLocalized(string name)
        {
            if (Category == null)
                return;

            Category.Name = name;
        }

        public void SetCityNameLocalized(string name)
        {
            if (City == null)
                return;

            City.Name = name;
        }
    }

    public class GetBannerResponse : BaseResponse<BannerPayloadItem>
    {
        public GetBannerResponse(
            int statusCode,
            string message,
            BannerPayloadItem? payload
        ) : base(statusCode, message, payload)
        {
        }
    }
}
