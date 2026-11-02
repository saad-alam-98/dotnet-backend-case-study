using Newtonsoft.Json;

namespace SaudiPlus.Response.Invoice
{
    public class GetOfferForInvoiceResponse : BaseResponse<GetOfferForInvoicePayload>
    {
        public GetOfferForInvoiceResponse(
                int statusCode,
                string message,
                GetOfferForInvoicePayload? payload
            ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetOfferForInvoicePayload
    {
        [JsonProperty("items")]
        public required List<GetOfferForInvoiceItem> Items;
    }

    public class GetOfferForInvoiceItem
    {
        [JsonProperty("id")]
        public required int Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }
    }
}
