using Newtonsoft.Json;
using SaudiPlus.Response.Category;

namespace SaudiPlus.Response.BusinessUser
{
    public abstract class BaseBusinessInfo
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("email")]
        public required string Email { get; set; }

        [JsonProperty("description")]
        public required string Description { get; set; }

        [JsonProperty("mobileNumber")]
        public required string MobileNumber { get; set; }

        [JsonProperty("phoneNumber")]
        public required string PhoneNumber { get; set; }

        [JsonProperty("address")]
        public required string Address { get; set; }

        [JsonProperty("logoPath")]
        public required string LogoPath { get; set; }

        [JsonProperty("creationTime")]
        public DateTime CreationTime { get; set; }

        [JsonProperty("lastModificationTime")]
        public DateTime? LastModificationTime { get; set; }
    }

    public class BusinessInfoDetailed : BaseBusinessInfo
    {
        [JsonProperty("isActive")]
        public required bool isActive { get; set; }

        [JsonProperty("nameAr")]
        public required string NameAr { get; set; }

        [JsonProperty("nameEn")]
        public required string nameEn { get; set; }

        [JsonProperty("howToUseAr")]
        public required string HowToUseAr { get; set; }

        [JsonProperty("howToUseEn")]
        public required string HowToUseEn { get; set; }

        [JsonProperty("category")]
        public required CategoryInfoDetailed Category { get; set; }
    }

    public class BusinessInfoLocalized : BaseBusinessInfo
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("howToUse")]
        public required string HowToUse { get; set; }

        [JsonProperty("category")]
        public required CategoryInfoLocalized Category { get; set; }
        
        [JsonProperty("rating")]
        public required double Rating { get; set; }

        [JsonProperty("review")]
        public required int Review { get; set; }

        public void SetCategoryNameLocalized(string name)
        {
            if (Category == null)
                return;

            Category.Name = name;
        }
    }
}
