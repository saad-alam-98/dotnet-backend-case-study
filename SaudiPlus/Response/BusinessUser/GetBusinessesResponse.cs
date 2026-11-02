using Newtonsoft.Json;
using SaudiPlus.Enums;

namespace SaudiPlus.Response.BusinessUser
{
    public class GetBusinessesResponse : BaseResponse<BusinessPayload>
    {
        public GetBusinessesResponse(
            int statusCode,
            string message,
            BusinessPayload? payload
        )
            : base(statusCode, message, payload)
        {
        }
    }

    public class BusinessPayload
    {
        [JsonProperty("items")]
        public required List<BaseBusinessInfo> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class GetBusinessResponse : BaseResponse<BaseBusinessInfo>
    {
        public GetBusinessResponse(
            int statusCode,
            string message,
            BaseBusinessInfo? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetBusinessOffersResponse : BaseResponse<GetBusinessOffersPayload>
    {
        public GetBusinessOffersResponse(
            int statusCode,
            string message,
            GetBusinessOffersPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetBusinessOffersPayload
    {
        [JsonProperty("items")]
        public required List<BusinessOfferItem> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class BusinessOfferItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public required string Title { get; set; }

        [JsonProperty("isActive")]
        public required bool IsActive { get; set; }

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

        [JsonProperty("isRedeemByUser")]
        public bool? IsRedeemByUser { get; set; }

        [JsonProperty("lastRedeemTime")]
        public DateTime? LastRedeemTime { get; set; }

        [JsonProperty("canRedeemAgainAt")]
        public DateTime? CanRedeemAgainAt { get; set; }

        [JsonProperty("notes")]
        public required string Notes { get; set; }

        [JsonProperty("points")]
        public int Points { get; set; }

        [JsonProperty("kmAway")]
        public double KmAway { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("tags")]
        public required List<OfferTag>? Tags { get; set; }

        [JsonProperty("offerUrl")]
        public required List<string> OfferUrl { get; set; }
    }

    public class OfferTag
    {
        [JsonProperty("type")]
        public OfferTags Type { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("tagColor")]
        public string TagColor { get; set; }

        public OfferTag(OfferTags type, string tag, string tagColor)
        {
            Tag = tag;
            TagColor = tagColor;
            Type = type;
        }
    }
}
