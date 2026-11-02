using SaudiPlus.Enums;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities;

public partial class Contract : Entity<int>
{
    public required Guid BusinessId { get; set; }
    public required string CommercialRegistrationNo { get; set; }
    public required string SupervisorName { get; set; }
    public required string SupervisorPhone { get; set; }
    public required string SupervisorEmail { get; set; }

    public required ContractType ContractType { get; set; } 

    public float? SubscriptionAmount { get; set; }
    public float? FixedAmountPerOffer { get; set; }
    public float? PercentageOfInvoice { get; set; }

    public virtual BusinessUser BusinessUser { get; set; } = null!;
}
