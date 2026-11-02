namespace SaudiPlus.Request.Account
{
    public class ValidateOtpDTO
    {
        public required string Otp { get; set; }
        public required string AccessToken { get; set; }
    }
}
