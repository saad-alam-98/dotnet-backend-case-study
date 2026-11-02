using Newtonsoft.Json;

namespace SaudiPlus.Request.Offer
{
    public class RedeemOfferByQrDTO
    {
        [JsonProperty("branchId")]
        public required int BranchId { get; set; }
    }
}
