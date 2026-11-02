using SaudiPlus.Enums;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public partial class Policy : Entity<int>
    {
        public PolicyType Type { get; set; }
        public required string ContentAr { get; set; }
        public required string ContentEn { get; set; }

        public string GetContentByLanguageCode(Language code)
        {
            return code == Language.ar ? ContentAr : ContentEn;
        }
    }
}
