using SaudiPlus.Entities.Notification;

namespace SaudiPlus.Interfaces.Repository
{
    public interface INotificationRepository : IRepository<BaseNotification, int>
    {
        Task<List<IndividualNotification>> GetIndividualNotifications();
        Task<List<GroupNotification>> GetGroupNotifications();
        Task<List<GeneralNotification>> GetGeneralNotifications();
        Task<IndividualNotification?> GetIndividualNotificationById(int id);
        Task<GroupNotification?> GetGroupNotificationById(int id);
        Task<GeneralNotification?> GetGeneralNotificationById(int id);
        Task<int> GetIndividualNotificationsCountForUser(Guid userId, string? searchKeyword);
        Task<int> GetGroupNotificationsCountForUser(Guid userId, string? searchKeyword);
        Task<int> GetGeneralNotificationsCountWithFilter(string? searchKeyword);
        Task<(List<NotificationDto> notifications, int totalCount)> GetAllNotificationsForUserByDatePaginated(Guid userId, string? searchKeyword, int pageNumber, int pageSize);
    }
}