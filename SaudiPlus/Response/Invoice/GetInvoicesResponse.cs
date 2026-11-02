using Newtonsoft.Json;

namespace SaudiPlus.Response.Invoice
{
    public class GetInvoicesResponse : BaseResponse<GetInvoicesPayload>
    {
        public GetInvoicesResponse(
            int statusCode,
            string message,
            GetInvoicesPayload? payload
        )
            : base(statusCode, message, payload)
        {
        }
    }

    public class GetInvoicesPayload
    {
        [JsonProperty("items")]
        public required List<InvoiceDetailPayload> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }
}
