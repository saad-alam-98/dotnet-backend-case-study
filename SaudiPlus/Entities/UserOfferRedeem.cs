using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public class UserOfferRedeem : Entity<int>
    {
        public required Guid UserId { get; set; }
        public required int OfferId { get; set; }

        public virtual IndividualUser User { get; set; } = null!;
        public virtual Offer Offer { get; set; } = null!;
    }
}
