
namespace SaudiPlus.Interfaces
{
    public interface ITextMessageService
    {
        Task<(bool, Exception?)> SendSms(string phoneNumber, string message);
    }
}
