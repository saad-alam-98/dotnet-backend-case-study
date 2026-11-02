using Newtonsoft.Json;
using SaudiPlus.Enums;
using SaudiPlus.Response.BusinessUser;

namespace SaudiPlus.Response.Offer
{
    public class GetOffersResponse : BaseResponse<GetOffersPayload>
    {
        public GetOffersResponse(
            int statusCode,
            string message,
            GetOffersPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetOfferResponse : BaseResponse<OfferPayloadItem>
    {
        public GetOfferResponse(
            int statusCode,
            string message,
            OfferPayloadItem? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetOffersPayload
    {
        [JsonProperty("items")]
        public required List<OfferPayloadItem> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class OfferPayloadItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public required string Title { get; set; }

        [JsonProperty("description")]
        public required string Description { get; set; }

        [JsonProperty("isActive")]
        public required bool isActive { get; set; }

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

        [JsonProperty("creationTime")]
        public DateTime CreationTime { get; set; }

        [JsonProperty("offerUrl")]
        public required List<string> OfferUrl { get; set; }

        [JsonProperty("business")]
        public BusinessInfoLocalized Business { get; set; } = null!;

        [JsonProperty("tags")]
        public required List<OfferTag>? Tags { get; set; }

        [JsonProperty("kmAway")]
        public double KmAway { get; set; }

        [JsonProperty("branchLatitude")]
        public double BranchLatitude { get; set; }

        [JsonProperty("branchLongitude")]
        public double BranchLongitude { get; set; }

        [JsonProperty("branchPlaceId")]
        public required string BranchPlaceId { get; set; }

        [JsonProperty("branchId")]
        public int branchId;

        public void SetBranchLocation(double latitude, double longitude, string PlaceId)
        {
            BranchLatitude = latitude;
            BranchLongitude = longitude;
            BranchPlaceId = PlaceId;
        }

        public void SetBusinessNameLocalized(string name)
        {
            if (Business == null)
                return;

            Business.Name = name;
        }
    }
}
