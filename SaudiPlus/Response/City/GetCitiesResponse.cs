using Newtonsoft.Json;

namespace SaudiPlus.Response.City
{
    public class GetCityResponse : BaseResponse<BaseCityInfo>
    {
        public GetCityResponse(
            int statusCode,
            string message,
            BaseCityInfo? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetCitiesResponse : BaseResponse<GetCityPayload>
    {
        public GetCitiesResponse(
            int statusCode, 
            string message,
            GetCityPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetCityPayload
    {
        [JsonProperty("items")]
        public required List<BaseCityInfo> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class BaseCityInfo
    {
        [JsonProperty("id")]
        public required int Id { get; set; }
    }

    public class CityInfoDetailed : BaseCityInfo
    {
        [JsonProperty("nameAr")]
        public string NameAr { get; set; } = null!;

        [JsonProperty("nameEn")]
        public string NameEn { get; set; } = null!;

        [JsonProperty("isActive")]
        public required bool IsActive { get; set; }
    }

    public class CityInfoLocalized : BaseCityInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; } = null!;
    }
}
