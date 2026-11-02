using Newtonsoft.Json;

namespace SaudiPlus.Request.Invoice
{
    public class CreateInvoiceDTO
    {
        [JsonProperty("branchId")]
        public required int BranchId { get; set; }

        [JsonProperty("amount")]
        public required int Amount { get; set; }

        [JsonProperty("mobileNumber")]
        public required string MobileNumber { get; set; }

        [JsonProperty("offerId")]
        public required int OfferId { get; set; }
    }
}
