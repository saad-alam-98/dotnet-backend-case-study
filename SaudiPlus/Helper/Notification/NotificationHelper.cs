using SaudiPlus.Response;
using SaudiPlus.Request.Notification;
using SaudiPlus.Entities.Notification;
using IndividualUserEntity = SaudiPlus.Entities.IndividualUser;

namespace SaudiPlus.Helper.Notification
{
    public static class NotificationHelper
    {
        public static (string? title, string message) GetLocalizedContent(
            IndividualUserEntity user, 
            string? titleAr,
            string? titleEn, 
            string messageAr, 
            string messageEn
        )
        {
            return user.Language == Enums.Language.ar ? (titleAr, messageAr) : (titleEn, messageEn);
        }

        public static SuccessResponse CreateSuccessResponse(string message)
        {
            return new SuccessResponse(
                StatusCodes.Status200OK,
                message,
                new SuccessResponsePayload { success = true }
            );
        }

        public static GroupNotification CreateGroupNotification(GroupUsersNotificationRequestDTO model, List<IndividualUserEntity> users)
        {
            var notification = new GroupNotification
            {
                TitleAr = model.TitleAr,
                TitleEn = model.TitleEn,
                MessageAr = model.MessageAr,
                MessageEn = model.MessageEn,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            foreach (var user in users)
            {
                notification.Users.Add(user);
            }
            
            return notification;
        }

        public static IndividualNotification CreateIndividualNotification(IndividualNotificationRequestDTO model, Guid userId, bool isSystemGenerated = false)
        {
            return new IndividualNotification
            {
                TitleAr = model.TitleAr,
                TitleEn = model.TitleEn,
                MessageAr = model.MessageAr,
                MessageEn = model.MessageEn,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsSystemGenerated = isSystemGenerated
            };
        }

        public static GeneralNotification CreateGeneralNotification(GeneralNotificationRequestDTO model)
        {
            return new GeneralNotification
            {
                TitleAr = model.TitleAr,
                TitleEn = model.TitleEn,
                MessageAr = model.MessageAr,
                MessageEn = model.MessageEn,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
