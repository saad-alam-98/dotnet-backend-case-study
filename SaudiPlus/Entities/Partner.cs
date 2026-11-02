using SaudiPlus.Enums;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities;

public partial class Partner : Entity<int>
{
    public Guid? BusinessUserId { get; set; }
    
    public string NameEn { get; set; } = null!; 

    public string NameAr { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MobileNumber { get; set; } = null!;

    public string? Password { get; set; }

    public string CompanyName { get; set; } = null!;

    public string LogoPath { get; set; } = null!;

    public int CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;
    public virtual BusinessUser BusinessUser { get; set; } = null!;

    public string GetNameByLanguageCode(Language code)
    {
        return code == Language.ar ? NameAr : NameEn;
    }

    public string GetCategoryNameLocalized(Language code)
    {
        if (Category == null)
            return "";

        return Category.GetNameByLanguageCode(code);
    }
}
