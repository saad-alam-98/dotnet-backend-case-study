using SaudiPlus.Enums;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities;

public partial class Category : Entity<int>
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string ImagePath { get; set; } = null!;
    public int? CityId { get; set; }
    public virtual City? City { get; set; }
    public virtual ICollection<BusinessUser> BusinessUsers { get; set; } = [];
    public virtual ICollection<Banner> Banners { get; set; } = [];
    public virtual ICollection<Partner> Partners { get; set; } = [];

    public string GetNameByLanguageCode(Language code)
    {
        return code == Language.ar ? NameAr : NameEn;
    }

    public string GetCityNameLocalized(Language code)
    {
        if (City == null)
            return "";

        return City.GetNameByLanguageCode(code);
    }
}
