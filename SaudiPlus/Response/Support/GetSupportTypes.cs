using Newtonsoft.Json;

namespace SaudiPlus.Response.Support
{
    public class GetSupportTypes : BaseResponse<GetSupportTypePayload>
    {
        public GetSupportTypes(
            int statusCode,
            string message,
            GetSupportTypePayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetSupportTypePayload
    {
        [JsonProperty("items")]
        public required List<BaseSupportTypeItem> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class BaseSupportTypeItem
    {
        [JsonProperty("id")]
        public required int Id { get; set; }

        [JsonProperty("isActive")]
        public required bool IsActive { get; set; }
    }

    public class SupportTypeItemDetailed : BaseSupportTypeItem
    {
        [JsonProperty("nameAr")]
        public string NameAr { get; set; } = null!;
        
        [JsonProperty("nameEn")]
        public string NameEn { get; set; } = null!;
    }

    public class SupportTypeItemLocalized : BaseSupportTypeItem
    {

        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        public void SetSupportTypeLocalized(string name)
        {
            Name = name;
        }
    }
}
