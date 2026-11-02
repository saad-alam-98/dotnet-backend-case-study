using SaudiPlus.Response;
using SaudiPlus.Entities;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Notification;
using SaudiPlus.Response.Notification;

namespace SaudiPlus.Interfaces.Service
{
    public interface INotificationCenterService
    {
        Task<NotificationUserResponse> GetUsersForNotification(EntityBySearchFilter filter);
        Task SendIndividualNotificationForget(IndividualNotificationRequestDTO model);
        Task<SuccessResponse> SendIndividualNotification(IndividualNotificationRequestDTO model);
        Task<SuccessResponse> SendGeneralNotification(GeneralNotificationRequestDTO model);
        Task<SuccessResponse> SendGroupNotification(GroupUsersNotificationRequestDTO model);

        Task<GetNotificationResponse> GetGroupNotifications(EntityFilter filter);
        Task<GetNotificationResponse> GetGeneralNotifications(EntityFilter filter);
        Task<GetNotificationResponse> GetIndividualNotifications(EntityFilter filter);


        Task<SuccessResponse> SendIndividualNotificationById(int id);
        Task<SuccessResponse> SendGroupNotificationById(int id);
        Task<SuccessResponse> SendGeneralNotificationById(int id);

        Task<GetSingleNotificationResponse> MyNotifications(IndividualUser userId, EntityFilter filter);
    }
}
