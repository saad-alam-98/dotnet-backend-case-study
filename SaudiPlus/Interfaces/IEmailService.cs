using SaudiPlus.Enums;

namespace SaudiPlus.Interfaces
{
    public interface IEmailService
    {
        public Task<(bool Success, string Message)> SendEmailAsync(string subject, string body, string email);

        string LoadEmailTemplate(string templateName, Language languageCode, Dictionary<string, string> placeholders);
    }
}
