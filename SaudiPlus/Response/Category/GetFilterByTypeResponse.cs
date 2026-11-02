using Newtonsoft.Json;
using SaudiPlus.Enums;
using SaudiPlus.Response.BusinessUser;

namespace SaudiPlus.Response.Category
{
    public class GetFilterByTypeResponse : BaseResponse<CategoryItemPayload>
    {
        public GetFilterByTypeResponse(
            int statusCode,
            string message,
            CategoryItemPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class CategoryItemPayload
    {
        [JsonProperty("type")]
        public required string Type { get; set; }

        [JsonProperty("items")]
        public required List<object> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class BusinessTypeItem
    {
        [JsonProperty("type")]
        public required Guid id;

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("logo")]
        public required string Logo { get; set; }

        [JsonProperty("category")]
        public required string Category { get; set; }

        [JsonProperty("rating")]
        public required double Rating { get; set; }

        [JsonProperty("review")]
        public required int Review { get; set; }

        [JsonProperty("kmAway")]
        public double KmAway { get; set; }
    }

    public class OfferTypeItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public required string Title { get; set; }

        [JsonProperty("description")]
        public required string Description { get; set; }

        [JsonProperty("offerUrl")]
        public required List<string> OfferUrl { get; set; }

        [JsonProperty("tags")]
        public required List<OfferTag>? Tags { get; set; }

        [JsonProperty("offerType")]
        public OfferType OfferType { get; set; }

        [JsonProperty("invoiceValue")]
        public float? InvoiceValue { get; set; }

        [JsonProperty("percentageValue")]
        public float? PercentageValue { get; set; }

        [JsonProperty("business")]
        public required BusinessTypeItem business { get; set; }
    }
}
