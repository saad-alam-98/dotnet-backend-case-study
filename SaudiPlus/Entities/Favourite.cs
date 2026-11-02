using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public class Favourite : Entity<int>
    {
        public Guid UserId { get; set; }
        public Guid BusinessUserId { get; set; }

        public virtual IndividualUser User { get; set; } = null!;
        public virtual BusinessUser Business { get; set; } = null!;
    }
}