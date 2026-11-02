using Newtonsoft.Json;
using SaudiPlus.Enums;

namespace SaudiPlus.Response.Contract
{
    public class GetContractResponse : BaseResponse<GetContractPayload>
    {
        public GetContractResponse(
            int statusCode,
            string message,
            GetContractPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetContractPayload
    {
        [JsonProperty("businessId")]
        public required Guid BusinessId { get; set; }

        [JsonProperty("commercialRegistrationNo")]
        public required string CommercialRegistrationNo { get; set; }

        [JsonProperty("supervisorName")]
        public required string SupervisorName { get; set; }

        [JsonProperty("supervisorPhone")]
        public required string SupervisorPhone { get; set; }

        [JsonProperty("supervisorEmail")]
        public required string SupervisorEmail { get; set; }

        [JsonProperty("contractType")]
        public required int ContractType { get; set; }

        [JsonProperty("subscriptionAmount")]
        public float? SubscriptionAmount { get; set; }

        [JsonProperty("offerAmount")]
        public float? OfferAmount { get; set; }

        [JsonProperty("invoicePercentage")]
        public float? InvoicePercentage { get; set; }
    }
}
