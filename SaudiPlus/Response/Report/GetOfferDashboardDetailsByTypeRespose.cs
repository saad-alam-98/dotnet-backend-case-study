using Newtonsoft.Json;

namespace SaudiPlus.Response.Report
{
    public class GetOfferDashboardDetailsByTypeRespose : BaseResponse<OfferDashboardDetailsByTypePayload>
    {
        public GetOfferDashboardDetailsByTypeRespose(
            int statusCode,
            string message,
            OfferDashboardDetailsByTypePayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class OfferDashboardDetailsByTypePayload
    {
        [JsonProperty("items")]
        public required List<BaseOfferDashboardDetailItem>? Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class BaseOfferDashboardDetailItem
    {
        [JsonProperty("offerTitle")]
        public required string OfferTitle { get; set; }

        [JsonProperty("offerId")]
        public required int OfferId { get; set; }

        [JsonProperty("businessTitle")]
        public required string BusinessTitle { get; set; }

        [JsonProperty("businessId")]
        public required Guid BusinessId { get; set; }
    }

    public class OfferDashboardDetailItem : BaseOfferDashboardDetailItem
    {
        [JsonProperty("rank")]
        public required int Rank { get; set; }

        [JsonProperty("usage")]
        public required int Usage { get; set; }

        [JsonProperty("number")]
        public required int Number { get; set; }
    }

    public class OfferDashboardUsageDetailItem : BaseOfferDashboardDetailItem
    {
        [JsonProperty("invoiceId")]
        public required int InvoiceId;

        [JsonProperty("invoiceNumber")]
        public required string InvoiceNumber;

        [JsonProperty("amount")]
        public required double Amount { get; set; }

        [JsonProperty("discount")]
        public required double Discount { get; set; }

        [JsonProperty("totalAmount")]
        public required double TotalAmount { get; set; }

        [JsonProperty("issueDate")]
        public required DateTime IssueDate { get; set; }

        [JsonProperty("userTitle")]
        public required string UserTitle { get; set; }

        [JsonProperty("userId")]
        public required Guid UserId { get; set; }
    }
}
