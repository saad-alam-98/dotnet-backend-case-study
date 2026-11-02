using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Request.Support
{
    public class CreateSupportTicketDTO
    {
        [JsonProperty("supportTypeId")]
        public required int SupportTypeId { get; set; }

        [JsonProperty("message")]
        [MaxLength(1000)]
        public required string Message { get; set; }
    }
}
