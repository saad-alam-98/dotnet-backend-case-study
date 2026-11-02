using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public class StatusInteraction : Entity<int>
    {
        public required Guid IndividualUserId { get; set; }
        public required int StatusId { get; set; }
        public bool IsViewed { get; set; }
        public bool IsClicked { get; set; }
        public int? InteractionTime { get; set; }

        public virtual IndividualUser IndividualUser { get; set; } = null!;
        public virtual Status Status { get; set; } = null!;
    }

}
