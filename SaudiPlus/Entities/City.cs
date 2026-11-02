using SaudiPlus.Enums;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities;

public partial class City : Entity<int>
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public virtual ICollection<Banner> Banners { get; set; } = [];
    public virtual ICollection<IndividualUser> Users { get; set; } = [];
    public virtual ICollection<Category> Categories { get; set; } = [];
    public virtual ICollection<Branch> Branches { get; set; } = [];

    public string GetNameByLanguageCode(Language code)
    {
        return code == Language.ar ? NameAr : NameEn;
    }
}
