using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities;

public partial class ContractCategory : Entity<int>
{
    public string CategoryName { get; set; } = null!;
}
