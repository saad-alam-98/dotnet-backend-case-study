namespace SaudiPlus.Request.Account
{
    public class ResetPasswordDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}
