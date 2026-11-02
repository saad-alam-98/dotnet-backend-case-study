using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public class BusinessRequest : Entity<int>
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string BusinessName { get; set; }
        public required string CategoryId { get; set; }
    }
}
