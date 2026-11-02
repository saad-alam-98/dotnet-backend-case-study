namespace SaudiPlus.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string targetArn, string? title, string body);
        Task PushNotificationAsync(string? title, string body);
        Task<string?> CreatePlatformEndpoint(string deviceToken);
    }
}
