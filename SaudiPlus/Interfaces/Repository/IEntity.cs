using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IEntity<TKey>
    {
        TKey Id { get; set; }
    }

    public class Entity<TId> : IEntity<TId>, IAuditedEntity
    {
        [Key]
        public virtual TId Id { get ; set; }
        public DateTime CreationTime { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
}
