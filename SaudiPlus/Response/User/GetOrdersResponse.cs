using Newtonsoft.Json;
using SaudiPlus.Enums;

namespace SaudiPlus.Response.User
{
    public class GetOrdersResponse : BaseResponse<GetUserOrdersPayload>
    {
        public GetOrdersResponse(
            int statusCode,
            string message,
            GetUserOrdersPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetUserOrdersPayload
    {
        [JsonProperty("items")]
        public required List<GetUserOrdersItems> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class GetUserOrdersItems
    {
        [JsonProperty("orderNumber")]
        public required string OrderNumber;

        [JsonProperty("amount")]
        public required double Amount { get; set; }

        [JsonProperty("discount")]
        public required double Discount { get; set; }

        [JsonProperty("totalAmount")]
        public required double TotalAmount { get; set; }

        [JsonProperty("issueDate")]
        public required DateTime IssueDate { get; set; }

        [JsonProperty("offer")]
        public required OrderOfferItem Offer { get; set; }
    }

    public class OrderOfferItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public required string Title { get; set; }

        [JsonProperty("offerType")]
        public OfferType OfferType { get; set; }

        [JsonProperty("invoiceValue")]
        public float? InvoiceValue { get; set; }

        [JsonProperty("percentageValue")]
        public float? PercentageValue { get; set; }

        [JsonProperty("points")]
        public int Points { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("offerUrl")]
        public required string OfferUrl { get; set; }
    }
}
