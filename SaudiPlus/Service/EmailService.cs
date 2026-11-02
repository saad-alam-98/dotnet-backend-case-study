using SendGrid;
using SaudiPlus.Enums;
using SaudiPlus.Interfaces;
using SendGrid.Helpers.Mail;
using SaudiPlus.Helper.Email;
using Microsoft.Extensions.Options;

namespace SaudiPlus.Service
{
    public class EmailService : IEmailService
    {
        private readonly SendGridSetting _setting;

        public EmailService(IOptions<SendGridSetting> setting)
        {
            _setting = setting.Value;
        }

        public async Task<(bool Success, string Message)> SendEmailAsync(string subject, string body, string address)
        {
            var client = new SendGridClient(_setting.ApiKey);
            var from = new EmailAddress(_setting.SenderEmail, _setting.SenderName);
            var to = new EmailAddress(address);
            
            // Create plain text version of the HTML content
            var plainTextContent = body.Replace("<h1>", "").Replace("</h1>", "\n").Replace("<p>", "").Replace("</p>", "\n");
            
            // Create the email message
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, body);
            
            // Add a reply-to address - this can help with deliverability
            msg.SetReplyTo(new EmailAddress("support@saudi-plus.com", "Saudi Plus Support"));
            
            try
            {
                Console.WriteLine($"Attempting to send email to {address} via SendGrid");
                var response = await client.SendEmailAsync(msg);
                var responseBody = await response.Body.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Email sent successfully!");
                }
                else
                {
                    return (false, $"Failed to send email: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public string LoadEmailTemplate(string templateName, Language languageCode, Dictionary<string, string> placeholders)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", $"{templateName}.{languageCode}.html");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Email template not found: {filePath}");

            var content = File.ReadAllText(filePath);

            foreach (var kvp in placeholders)
            {
                content = content.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
            }

            return content;
        }
    }
}
