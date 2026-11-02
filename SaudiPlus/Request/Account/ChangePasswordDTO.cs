using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Request.Account
{
    public class ChangePasswordDTO
    {
        public required string AccessToken { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}
