using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public partial class OfferLogo : Entity<int>
    {
        public int OfferId { get; set; }
        public required string LogoUrl { get; set; }
        public virtual Offer Offer { get; set; } = null!;
    }
}
