using Newtonsoft.Json;

namespace SaudiPlus.Response.Invoice
{
    public class GetBranchForInvoice : BaseResponse<BranchForInvoicePayload>
    {
        public GetBranchForInvoice(
            int statusCode,
            string message,
            BranchForInvoicePayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class BranchForInvoicePayload
    {
        [JsonProperty("items")]
        public required List<InvoiceBranchDetail> Items { get; set; }
    }

    public class InvoiceBranchDetail
    {
        [JsonProperty("id")]
        public required int Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("branchNumber")]
        public required int BranchNumber { get; set; }

        [JsonProperty("address")]
        public required string Address { get; set; }

        [JsonProperty("qrCodePath")]
        public required string QrCodePath { get; set; }
    }
}
