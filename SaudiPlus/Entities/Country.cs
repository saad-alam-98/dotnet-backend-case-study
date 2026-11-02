using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities;

public partial class Country : Entity<int>
{
    public int CountryId { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public virtual ICollection<City> Cities { get; set; } = [];
}
