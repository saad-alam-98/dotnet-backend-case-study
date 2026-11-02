using Microsoft.AspNetCore.Identity;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities.Identity
{
    public class User : IdentityUser<Guid>, IAuditedEntity
    {
        public bool IsActive { get; set; } = true;
        public bool IsGuest { get; set; }
        public string? DeviceIdentifier { get; set; }
        public string? NumberConfimationToken { get; set; }
        public DateTime? NumberConfimationExpiry { get; set; }
        public string? CurrentRefreshToken { get; set; }
        public string? PreviousRefreshToken { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastModificationTime { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
