using Newtonsoft.Json;
using SaudiPlus.Enums;
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Response.Support
{
    public class GetTicketsResponse : BaseResponse<GetTicketsPayload>
    {
        public GetTicketsResponse(
            int statusCode,
            string message,
            GetTicketsPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetTicketsPayload
    {
        [JsonProperty("items")]
        public required List<GetTicketItemPayload> Items;
    }
    public class GetTicketsPayloadPaginated : GetTicketsPayload
    {
        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class CreateTicketResponse : BaseResponse<GetTicketPayload>
    {
        public CreateTicketResponse(
            int statusCode,
            string message,
            GetTicketPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class GetTicketPayload
    {
        [JsonProperty("id")]
        public required string Id;

        [JsonProperty("message")]
        [MaxLength(1000)]
        public required string Message { get; set; }

        [JsonProperty("ticketStatus")]
        public required TicketStatus TicketStatus { get; set; }

        [JsonProperty("supportType")]
        public required SupportTypeItemLocalized SupportType { get; set; }

        [JsonProperty("messages")]
        public required List<TicketMessagePayload> Messages { get; set; }
    }

    public class TicketMessagePayload
    {
        [JsonProperty("message")]
        public required string Message { get; set; }

        [JsonProperty("isAdmin")]
        public required bool IsAdmin { get; set; }

        [JsonProperty("sentAt")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }

    public class GetTicketItemPayload
    {
        [JsonProperty("id")]
        public required string Id;

        [JsonProperty("ticketStatus")]
        public required TicketStatus TicketStatus { get; set; }

        [JsonProperty("supportType")]
        public required SupportTypeItemLocalized SupportType { get; set; }

        [JsonProperty("user")]
        public required TicketUserInfo User { get; set; }

        [JsonProperty("messages")]
        public required List<TicketMessagePayload> Messages { get; set; }
    }

    public class TicketUserInfo
    {
        [JsonProperty("displayName")]
        public required string DisplayName { get; set; }

        [JsonProperty("avatarUrl")]
        public string? AvatarUrl { get; set; }
    }
}
