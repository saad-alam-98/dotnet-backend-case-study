using Newtonsoft.Json;
using SaudiPlus.Response.Category;

namespace SaudiPlus.Response.Partner
{
    public class GetPartnerResponse : BaseResponse<BasePartnerInfoPayload>
    {
        public GetPartnerResponse(
            int statusCode,
            string message,
            BasePartnerInfoPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class BasePartnerInfoPayload
    {
        [JsonProperty("id")]
        public required int Id { get; set; }

        [JsonProperty("companyName")]
        public required string CompanyName { get; set; }

        [JsonProperty("email")]
        public required string Email { get; set; }

        [JsonProperty("password")]
        public required string Password { get; set; }

        [JsonProperty("mobileNumber")]
        public required string MobileNumber { get; set; }

        [JsonProperty("isActive")]
        public required bool IsActive { get; set; }

        [JsonProperty("creationTime")]
        public DateTime CreationTime { get; set; }

        [JsonProperty("category")]
        public required CategoryInfoLocalized Category { get; set; }

        [JsonProperty("logoPath")]
        public required string LogoPath { get; set; }

        public void SetCategoryNameLocalized(string name)
        {
            if (Category == null)
                return;

            Category.Name = name;
        }
    }

    public class PartnerInfoDetailPayload : BasePartnerInfoPayload
    {

        [JsonProperty("nameAr")]
        public required string NameAr { get; set; } 
        
        [JsonProperty("nameEn")]
        public required string NameEn { get; set; }
    }

    public class PartnerInfoLocalizedPayload : BasePartnerInfoPayload
    {

        [JsonProperty("name")] 
        public required string Name { get; set; }
    }
}
