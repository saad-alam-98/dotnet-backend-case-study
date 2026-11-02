using SaudiPlus.Enums;

namespace SaudiPlus.Request.IndividualUser
{
    public class CreateUserDTO
    {
        public required string email { get; set; }
        public required string password { get; set; }
        public required string displayName { get; set; }
        public string? username { get; set; }
        public required string phoneNumber { get; set; }
        public string? address { get; set; }
        public string? deviceToken { get; set; }
        public DateTime dateOfBirth { get; set; }
        public Gender gender { get; set; }
        public int cityId { get; set; }
    }
}
