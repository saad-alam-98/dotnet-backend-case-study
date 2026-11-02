using SaudiPlus.Enums;
using SaudiPlus.Interfaces.Repository;
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Entities;

public partial class Banner : Entity<int>
{
    [MaxLength(200)] public string Title { get; set; } = null!;
    public string ImagePath { get; set; } = null!;
    public Guid? BusinessId { get; set; }
    public virtual BusinessUser? Business { get; set; }
    public int? CategoryId { get; set; }
    public virtual Category? Category { get; set; }
    public int? CityId { get; set; }
    public virtual City? City { get; set; }


    public string GetBusinessNameLocalized(Language code)
    {
        if (Business == null)
            return "";

        return Business.GetNameByLanguageCode(code);
    }

    public string GetCategoryNameLocalized(Language code)
    {
        if (Category == null)
            return "";

        return Category.GetNameByLanguageCode(code);
    }

    public string GetCityNameLocalized(Language code)
    {
        if (City == null)
            return "";

        return City.GetNameByLanguageCode(code);
    }
}
