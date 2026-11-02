namespace SaudiPlus.Helper.Email
{
    public class SendGridSetting
    {
        public required string ApiKey { get; set; }
        public required string SenderEmail { get; set; }
        public required string SenderName { get; set; }
    }
}
