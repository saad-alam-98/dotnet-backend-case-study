using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public class Wallet : Entity<int>
    {
        public required Guid BusinessId { get; set; }
        public virtual BusinessUser BusinessUser { get; set; } = null!;
    }
}
