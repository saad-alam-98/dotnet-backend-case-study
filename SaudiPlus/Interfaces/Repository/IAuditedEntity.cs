namespace SaudiPlus.Interfaces.Repository
{
    public interface IAuditedEntity
    {
        DateTime CreationTime { get; set; }
        DateTime? LastModificationTime { get; set; }
    }
}
