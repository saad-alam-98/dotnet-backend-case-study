using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public class UserOfferToken : Entity<int>
    {
        public required Guid UserId { get; set; }
        public required int BranchId { get; set; }
        public required DateTime ExpiryTime { get; set; }
        public required string Code { get; set; }
        public virtual IndividualUser User { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
    }
}
