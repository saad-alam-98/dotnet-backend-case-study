using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Request.Support
{
    public class ReplyTicketDTO
    {
        [JsonProperty("ticketId")]
        public required int TicketId { get; set; }

        [JsonProperty("message")]
        [MaxLength(1000)]
        public required string Message { get; set; }
    }
}
