using Newtonsoft.Json;
using SaudiPlus.Enums;

namespace SaudiPlus.Request.Contract
{
    public class EditContractDTO
    {
        [JsonProperty("businessId")]
        public required Guid BusinessId { get; set; }

        [JsonProperty("supervisorName")]
        public string? SupervisorName { get; set; }

        [JsonProperty("supervisorPhone")]
        public string? SupervisorPhone { get; set; }

        [JsonProperty("supervisorEmail")]
        public string? SupervisorEmail { get; set; }

        [JsonProperty("contractType")]
        public int? ContractType { get; set; }

        [JsonProperty("subscriptionAmount")]
        public float? SubscriptionAmount { get; set; }

        [JsonProperty("fixedAmountPerOffer")]
        public float? FixedAmountPerOffer { get; set; }

        [JsonProperty("percentageOfInvoice")]
        public float? PercentageOfInvoice { get; set; }
    }
}
    