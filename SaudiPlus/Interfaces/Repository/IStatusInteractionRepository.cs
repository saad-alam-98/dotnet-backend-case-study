using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IStatusInteractionRepository : IRepository<StatusInteraction, int>
    {
        Task<StatusInteraction?> GetByStatusAndUserAsync(int statusId, Guid userId);
        Task<List<StatusInteraction>> GetStatusInteractions(int statusId);
    }
}
