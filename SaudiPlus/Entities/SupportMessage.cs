using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public class SupportMessage : Entity<int>
    {
        public required int TicketId { get; set; }
        public required Guid UserId { get; set; }
        public bool IsAdmin { get; set; }
        public required string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public virtual SupportTicket Ticket { get; set; }
        public virtual IndividualUser User { get; set; }
    }
}
