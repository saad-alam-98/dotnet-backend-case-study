using SaudiPlus.Enums;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public class SupportTicket : Entity<int>
    {
        public required Guid UserId { get; set; }
        public virtual IndividualUser User { get; set; }
        public required int SupportTypeId { get; set; }
        public virtual SupportType SupportType { get; set; }
        public required TicketStatus TicketStatus { get; set; }
        public ICollection<SupportMessage> Messages { get; set; }
    }
}
