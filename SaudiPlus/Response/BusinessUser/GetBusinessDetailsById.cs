using Newtonsoft.Json;
using SaudiPlus.Enums;
using SaudiPlus.Response.Policy;

namespace SaudiPlus.Response.BusinessUser
{
    public class GetBusinessDetailsById : BaseResponse<GetBusinessDetailsPayload>
    {
        public GetBusinessDetailsById(
            int statusCode,
            string message,
            GetBusinessDetailsPayload? payload
        )
            : base(statusCode, message, payload)
        {
        }
    }

    public class GetBusinessDetailsPayload
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("email")]
        public required string Email { get; set; }

        [JsonProperty("description")]
        public required string Description { get; set; }

        [JsonProperty("mobileNumber")]
        public required string MobileNumber { get; set; }

        [JsonProperty("favourite")]
        public bool Favourite { get; set; }

        [JsonProperty("logoPath")]
        public required string LogoPath { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; } 
        
        [JsonProperty("androidAppUrl")]
        public string? AndroidAppUrl { get; set; }

        [JsonProperty("iOSAppUrl")]
        public string? iOSAppUrl { get; set; }

        [JsonProperty("websiteURL")]
        public string? WebsiteURL { get; set; }

        [JsonProperty("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonProperty("facebookUrl")]
        public string? FacebookUrl { get; set; }

        [JsonProperty("facebookPage")]
        public string? FacebookPage { get; set; }

        [JsonProperty("instagramUrl")]
        public string? InstagramUrl { get; set; }

        [JsonProperty("instagramPage")]
        public string? InstagramPage { get; set; }

        [JsonProperty("snapchatUrl")]
        public string? SnapchatUrl { get; set; }

        [JsonProperty("snapchatPage")]
        public string? SnapchatPage { get; set; }

        [JsonProperty("twitterUrl")]
        public string? TwitterUrl { get; set; }

        [JsonProperty("twitterPage")]
        public string? TwitterPage { get; set; }

        [JsonProperty("rating")]
        public double? Rating { get; set; }

        [JsonProperty("review")]
        public int? Review { get; set; }

        [JsonProperty("howToUse")]
        public required string HowToUse { get; set; }

        [JsonProperty("offers")]
        public required List<GetBusinessOffersDetails> Offers { get; set; }

        [JsonProperty("branches")]
        public required List<GetBusinessBranchDetails> Branches { get; set; }
        
        [JsonProperty("termsAndConditions")]
        public required PolicyLocalized TermsAndConditions { get; set; } 
        
        [JsonProperty("privacyPolicy")]
        public required PolicyLocalized PrivacyPolicy { get; set; }
    }

    public class GetBusinessOffersDetails
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public required string Title { get; set; }

        [JsonProperty("description")]
        public required string Description { get; set; }

        [JsonProperty("offerType")]
        public OfferType OfferType { get; set; }

        [JsonProperty("invoiceValue")]
        public float? InvoiceValue { get; set; }

        [JsonProperty("percentageValue")]
        public float? PercentageValue { get; set; }

        [JsonProperty("redeemByCode")]
        public bool? RedeemByCode { get; set; }

        [JsonProperty("redeemCode")]
        public string? RedeemCode { get; set; }

        [JsonProperty("notes")]
        public required string Notes { get; set; }

        [JsonProperty("points")]
        public int Points { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("offerUrl")]
        public required List<string> OfferUrl { get; set; }
    }

    public class GetBusinessBranchDetails
    {
        [JsonProperty("id")]
        public required int Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("branchNumber")]
        public required int BranchNumber { get; set; }

        [JsonProperty("address")]
        public required string Address { get; set; }

        [JsonProperty("placeId")]
        public required string PlaceId { get; set; }

        [JsonProperty("kmAway")]
        public double KmAway { get; set; }
    }
}
