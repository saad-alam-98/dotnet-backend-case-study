using Newtonsoft.Json;

namespace SaudiPlus.Request.Invoice
{
    public class SaveInvoiceDTO
    {
        [JsonProperty("invoiceId")]
        public required int InvoiceId { get; set; }
    }
}
