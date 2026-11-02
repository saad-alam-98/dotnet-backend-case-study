using Microsoft.AspNetCore.Identity;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities.Identity
{
    public class Role : IdentityRole<Guid>, IAuditedEntity
    {
        public Role() : base()
        {
            CreationTime = DateTime.Now;
        }

        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid? CreatorId { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public Guid? LastModifierId { get; set; }

        public bool IsActive { get; set; }

    }
}
