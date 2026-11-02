using SaudiPlus.Entities.Identity;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public class UserPermission : Entity<int>
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string PermissionId { get; set; } = null!;
    }
}
