using SaudiPlus.Enums;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities;

public partial class SupportType : Entity<int>
{
    public required string NameAr { get; set; }

    public required string NameEn { get; set; }

    public string GetNameByLanguageCode(Language code)
    {
        return code == Language.ar ? NameAr : NameEn;
    }
}
