
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Request.BusinessUser
{
    public class CreateBusinessDTO
    {
        public required string Email { get; set; }
        [MaxLength(50)] public required string Name { get; set; }
        public required string PhoneNumber { get; set; }
        [MaxLength(50)] public required string CompanyName { get; set; }
    }
}
