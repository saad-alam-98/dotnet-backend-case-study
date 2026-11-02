using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities.Notification
{
    public abstract class BaseNotification : Entity<int>
    {
        public string? TitleAr { get; set; }
        public string? TitleEn { get; set; }
        public required string MessageAr { get; set; }
        public required string MessageEn { get; set; }
        public required DateTime CreatedAt { get; set; }
        public bool IsSystemGenerated { get; set; }
    }

    public class IndividualNotification : BaseNotification
    {
        public required Guid UserId { get; set; }
        public virtual IndividualUser User { get; set; }
    }

    public class GroupNotification : BaseNotification
    {
        public virtual ICollection<IndividualUser> Users { get; set; } = new List<IndividualUser>();
    }

    public class GeneralNotification : BaseNotification { }
}
