using SaudiPlus.Enums;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities
{
    public partial class BusinessPolicy : Entity<int>
    {
        public Guid BusinessId { get; set; }
        public PolicyType Type { get; set; }
        public required string ContentAr { get; set; }
        public required string ContentEn { get; set; }
        public required virtual BusinessUser Business { get; set; }

        public string GetContentByLanguageCode(Language code)
        {
            return code == Language.ar ? ContentAr : ContentEn;
        }
    }
}
