using Newtonsoft.Json;
using SaudiPlus.Enums;

namespace SaudiPlus.Request.Contract
{
    public class CreateContractDTO
    {
        [JsonProperty("businessId")]
        public required Guid BusinessId { get; set; }

        [JsonProperty("commercialRegistrationNo")]
        public required string CommercialRegistrationNo { get; set; } = null!;

        [JsonProperty("supervisorName")]
        public required string SupervisorName { get; set; } = null!;

        [JsonProperty("supervisorPhone")]
        public required string SupervisorPhone { get; set; } = null!;

        [JsonProperty("supervisorEmail")]
        public required string SupervisorEmail { get; set; } = null!;

        [JsonProperty("contractType")]
        public required int ContractType { get; set; }

        [JsonProperty("subscriptionAmount")]
        public float? SubscriptionAmount { get; set; }

        [JsonProperty("fixedAmountPerOffer")]
        public float? FixedAmountPerOffer { get; set; }

        [JsonProperty("percentageOfInvoice")]
        public float? PercentageOfInvoice { get; set; }
    }
}
