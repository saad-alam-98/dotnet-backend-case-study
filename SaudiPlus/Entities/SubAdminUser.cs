using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities;

public partial class SubAdminUser : Entity<Guid>
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public virtual ICollection<UserPermission> Permissions { get; set; } = [];
}
