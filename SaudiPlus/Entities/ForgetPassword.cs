using SaudiPlus.Entities.Identity;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public class ForgetPassword : Entity<int>
    {
        public required string Email { get; set; }
        public required string Otp { get; set; }
        public DateTime ExpiryTime { get; set; }
        public required string AccessToken { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
    }
}
