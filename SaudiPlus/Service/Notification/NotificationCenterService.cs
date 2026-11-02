using AutoMapper;
using SaudiPlus.Response;
using SaudiPlus.Entities;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Helper.Notification;
using SaudiPlus.Request.Notification;
using SaudiPlus.Response.Notification;
using SaudiPlus.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace SaudiPlus.Service.Notification
{
    public class NotificationCenterService : BaseService<NotificationCenterService>, INotificationCenterService
    {
        private readonly SaudiPlusDbContext _dbContext;
        private readonly INotificationService _notificationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IIndividualUserRepository _individualUserRepository;

        public NotificationCenterService(
            IMapper mapper,
            SaudiPlusDbContext dbContext,
            ILanguageService languageService,
            INotificationService notificationService,
            ILogger<NotificationCenterService> logger,
            INotificationRepository notificationRepository,
            IIndividualUserRepository individualUserRepository
        ) : base(mapper, logger, languageService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
            _notificationRepository = notificationRepository;
            _individualUserRepository = individualUserRepository;
        }

        public async Task<SuccessResponse> SendIndividualNotification(IndividualNotificationRequestDTO model)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                return await ExecuteInTransaction(async () =>
                {
                    var user = await 
                        _individualUserRepository.GetUserWithRelationsByEmail(model.Email) ?? 
                        throw new Exception("User does not exist");

                    await SendNotificationToUser(
                        user,
                        model.TitleAr,
                        model.TitleEn,
                        model.MessageAr,
                        model.MessageEn
                    );

                    var notification = NotificationHelper.CreateIndividualNotification(model, user.Id);
                    await _notificationRepository.InsertAsync(notification, true);

                    return NotificationHelper.CreateSuccessResponse("Notification Sent");
                });
            }, "An error occurred while sending notification");
        }

        public async Task<SuccessResponse> SendGroupNotification(GroupUsersNotificationRequestDTO model)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                return await ExecuteInTransaction(async () =>
                {
                    var users = 
                        (await GetValidUsers(model.Emails))
                        .Where(u => u.IsActive && !u.IsDelete)
                        .ToList();

                    var usersWithDeviceArn = await Task.WhenAll(users.Select(async x => new
                    {
                        User = x,
                        DeviceArn = await EnsureDeviceArn(x)
                    }));

                    if (users.Count == 0) throw new Exception("No users found");

                    await SendNotificationToUsers(
                        users,
                        model.TitleAr,
                        model.TitleEn,
                        model.MessageAr,
                        model.MessageEn
                    );

                    var notification = NotificationHelper.CreateGroupNotification(model, users);
                    await _notificationRepository.InsertAsync(notification, true);

                    return NotificationHelper.CreateSuccessResponse("Notification Sent");
                });
            }, "An error occurred while sending group notification");
        }

        public async Task<SuccessResponse> SendGeneralNotification(GeneralNotificationRequestDTO model)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                await _notificationService.PushNotificationAsync(
                    model.TitleEn,
                    model.MessageEn
                );

                var notification = NotificationHelper.CreateGeneralNotification(model);
                await _notificationRepository.InsertAsync(notification, true);

                return NotificationHelper.CreateSuccessResponse("Group notification queued for processing");
            }, "An error occurred while sending general notification");
        }

        public async Task SendIndividualNotificationForget(IndividualNotificationRequestDTO model)
        {
            try
            {
                var user = await _individualUserRepository.GetUserWithRelationsByEmail(model.Email);
                if (user != null)
                {
                    await SendNotificationToUser(
                        user,
                        model.TitleAr,
                        model.TitleEn,
                        model.MessageAr,
                        model.MessageEn
                    );

                    var notification = NotificationHelper.CreateIndividualNotification(model, user.Id, true);
                    await _notificationRepository.InsertAsync(notification, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending notification");
            }
        }

        public async Task<SuccessResponse> SendIndividualNotificationById(int id)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                return await ExecuteInTransaction(async () =>
                {
                    var notification = await _notificationRepository.GetIndividualNotificationById(id) ?? throw new Exception("Notification not found");
                    if (notification.User == null) throw new Exception("User not found for this notification");

                    await SendNotificationToUser(
                        notification.User,
                        notification.TitleAr,
                        notification.TitleEn,
                        notification.MessageAr,
                        notification.MessageEn
                    );

                    notification.CreatedAt = DateTime.UtcNow;
                    await _notificationRepository.UpdateAsync(notification, true);

                    return NotificationHelper.CreateSuccessResponse("Notification Sent");
                });
            }, "An error occurred while sending individual notification by ID");
        }

        public async Task<SuccessResponse> SendGroupNotificationById(int id)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                return await ExecuteInTransaction(async () =>
                {
                    var notification = 
                        await _notificationRepository.GetGroupNotificationById(id) ?? 
                        throw new Exception("Notification not found");
                    
                    var users = notification.Users.ToList();
                    if (users.Count == 0) throw new Exception("No users found for this notification");

                    await SendNotificationToUsers(
                        users,
                        notification.TitleAr,
                        notification.TitleEn,
                        notification.MessageAr,
                        notification.MessageEn
                    );

                    notification.CreatedAt = DateTime.UtcNow;
                    await _notificationRepository.UpdateAsync(notification, true);

                    return NotificationHelper.CreateSuccessResponse("Group Notification Sent");
                });
            }, "An error occurred while sending group notification by ID");
        }

        public async Task<SuccessResponse> SendGeneralNotificationById(int id)
        {
            return await ExecuteWithErrorHandling(async () =>
            {
                var notification = 
                    await _notificationRepository.GetGeneralNotificationById(id) ?? 
                    throw new Exception("Notification not found");

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _notificationService.PushNotificationAsync(
                            notification.TitleEn,
                            notification.MessageEn
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Notification error: {ex.Message}");
                    }
                });

                notification.CreatedAt = DateTime.UtcNow;
                await _notificationRepository.UpdateAsync(notification, true);

                return NotificationHelper.CreateSuccessResponse("General notification sent");
            }, "An error occurred while sending general notification by ID");
        }

        private async Task<string?> EnsureDeviceArn(Entities.IndividualUser user)
        {
            if (string.IsNullOrEmpty(user.DeviceArn) && user.DeviceToken != null)
            {
                try
                {
                    var targetArn = await _notificationService.CreatePlatformEndpoint(user.DeviceToken);
                    if (string.IsNullOrEmpty(targetArn))
                        return null;

                    user.DeviceArn = targetArn;
                    await _individualUserRepository.UpdateAsync(user, true);
                    return targetArn;
                }
                catch
                {
                    return null;
                }
            }
            return user.DeviceArn;
        }

        private async Task SendNotificationToUser(
            Entities.IndividualUser user, 
            string? titleAr, 
            string? titleEn,
            string messageAr, 
            string messageEn
        )
        {
            var targetArn = await EnsureDeviceArn(user);
            if (string.IsNullOrEmpty(targetArn))
            {
                throw new Exception("Device not registered");
            }

            var (title, message) = NotificationHelper.GetLocalizedContent(
                user, 
                titleAr, 
                titleEn, 
                messageAr, 
                messageEn
            );

            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.SendNotificationAsync(targetArn, title, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification error: {ex.Message}");
                }
            });
        }

        private async Task SendNotificationToUsers(
            IEnumerable<Entities.IndividualUser> users, 
            string? titleAr,
            string? titleEn, 
            string messageAr, 
            string messageEn
        )
        {
            foreach (var user in users)
            {
                try
                {
                    await SendNotificationToUser(
                        user, 
                        titleAr, 
                        titleEn, 
                        messageAr, 
                        messageEn
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to send notification to user {user.Id}");
                }
            }
        }

        private async Task<List<Entities.IndividualUser>> GetValidUsers(IEnumerable<string> emails)
        {
            var users = new List<Entities.IndividualUser>();
            foreach (string email in emails)
            {
                var user = await _individualUserRepository.GetUserWithRelationsByEmail(email);
                if (user != null) users.Add(user);
            }
            return users;
        }

        private async Task<T> ExecuteInTransaction<T>(Func<Task<T>> operation)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync<object, T>(
                null,
                async (context, state, cancellationToken) =>
                {
                    await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                    try
                    {
                        var result = await operation();
                        await transaction.CommitAsync(cancellationToken);
                        return result;
                    }
                    catch
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw;
                    }
                },
                null,
                CancellationToken.None);
        }

        private async Task<SuccessResponse> ExecuteWithErrorHandling(Func<Task<SuccessResponse>> operation, string errorMessage)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorMessage);
                return new SuccessResponse(
                    StatusCodes.Status500InternalServerError,
                    ex.Message,
                    new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetNotificationResponse> GetGroupNotifications(EntityFilter filter)
        {
            try
            {
                var notifications = await _notificationRepository.GetGroupNotifications();
                var query = notifications.AsQueryable();

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(n =>
                        n.TitleEn!.ToLower().Contains(searchKeyword) ||
                        n.TitleAr!.ToLower().Contains(searchKeyword) ||
                        n.MessageEn.ToLower().Contains(searchKeyword) ||
                        n.MessageAr.ToLower().Contains(searchKeyword));
                }

                var totalItems = query.Count();
                var paginatedNotifications = query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToList();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new NotificationsPayload
                {
                    Items = [],
                    TotalItems = paginatedNotifications.Count,
                    HasNextPage = hasNextPage,
                };

                foreach (var notification in paginatedNotifications)
                {
                    var item = _mapper.Map<GroupNotificationPayload>(notification);
                    payload.Items.Add(item);
                }

                return new GetNotificationResponse(
                    StatusCodes.Status200OK,
                    "Group notifications retrieved successfully",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new GetNotificationResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<GetNotificationResponse> GetGeneralNotifications(EntityFilter filter)
        {
            try
            {
                var notifications = await _notificationRepository.GetGeneralNotifications();
                var query = notifications.AsQueryable();

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(n =>
                        n.TitleEn!.ToLower().Contains(searchKeyword) ||
                        n.TitleAr!.ToLower().Contains(searchKeyword) ||
                        n.MessageEn.ToLower().Contains(searchKeyword) ||
                        n.MessageAr.ToLower().Contains(searchKeyword));
                }

                var totalItems = query.Count();
                var paginatedNotifications = query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToList();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new NotificationsPayload
                {
                    Items = [],
                    TotalItems = paginatedNotifications.Count,
                    HasNextPage = hasNextPage,
                };

                foreach (var notification in paginatedNotifications)
                {
                    var item = _mapper.Map<GeneralNotificationPayload>(notification);
                    payload.Items.Add(item);
                }

                return new GetNotificationResponse(
                    StatusCodes.Status200OK,
                    "General notifications retrieved successfully",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new GetNotificationResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<GetNotificationResponse> GetIndividualNotifications(EntityFilter filter)
        {
            try
            {
                var notifications = await _notificationRepository.GetIndividualNotifications();
                var query = notifications.AsQueryable().Where(n => !n.IsSystemGenerated);

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(n =>
                        n.TitleEn!.ToLower().Contains(searchKeyword) ||
                        n.TitleAr!.ToLower().Contains(searchKeyword) ||
                        n.MessageEn.ToLower().Contains(searchKeyword) ||
                        n.MessageAr.ToLower().Contains(searchKeyword));
                }

                var totalItems = query.Count();
                var paginatedNotifications = query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToList();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new NotificationsPayload
                {
                    Items = [],
                    TotalItems = paginatedNotifications.Count,
                    HasNextPage = hasNextPage,
                };

                foreach (var notification in paginatedNotifications)
                {
                    var item = _mapper.Map<IndividualNotificationPayload>(notification);
                    payload.Items.Add(item);
                }

                return new GetNotificationResponse(
                    StatusCodes.Status200OK,
                    "Individual notifications retrieved successfully",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new GetNotificationResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<NotificationUserResponse> GetUsersForNotification(EntityBySearchFilter filter)
        {
            try
            {
                var users = _individualUserRepository.GetUsersForFilter()
                .Where(u => u.IsActive && !u.IsDelete);

                var userList = await users.ToListAsync();
                var usersWithDeviceArn = await Task.WhenAll(userList.Select(async x => new
                {
                    User = x,
                    DeviceArn = await EnsureDeviceArn(x)
                }));

                var filteredUsers = usersWithDeviceArn
                .Where(x => !string.IsNullOrEmpty(x.DeviceArn))
                .Select(x => x.User)
                .AsQueryable();

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    filteredUsers = filteredUsers.Where(u =>
                        u.DisplayName.ToLower().Contains(searchKeyword) ||
                        u.Email.ToLower().Contains(searchKeyword)
                    );
                }

                var payload = new NotificationUsersPayload
                {
                    Items = []
                };

                foreach (var user in filteredUsers)
                {
                    var item = _mapper.Map<NotificationUserItem>(user);
                    payload.Items.Add(item);
                }

                return new NotificationUserResponse(
                    StatusCodes.Status200OK,
                    "Notification Users",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new NotificationUserResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<GetSingleNotificationResponse> MyNotifications(Entities.IndividualUser user, EntityFilter filter)
        {
            try
            {
                var individualCount = await _notificationRepository.GetIndividualNotificationsCountForUser(user.Id, filter.searchKeyword);
                var groupCount = await _notificationRepository.GetGroupNotificationsCountForUser(user.Id, filter.searchKeyword);
                var generalCount = await _notificationRepository.GetGeneralNotificationsCountWithFilter(filter.searchKeyword);

                var totalEstimate = individualCount + groupCount + generalCount;

                if (totalEstimate == 0)
                {
                    return new GetSingleNotificationResponse(
                        StatusCodes.Status200OK,
                        "No notifications found",
                        new NotificationsSinglePayload
                        {
                            Items = new List<NotificationSingleItem>(),
                            TotalItems = 0,
                            HasNextPage = false
                        }
                    );
                }

                var (notifications, totalItems) = await _notificationRepository.GetAllNotificationsForUserByDatePaginated(
                    user.Id,
                    filter.searchKeyword,
                    filter.pageNumber,
                    filter.pageSize
                );

                if (notifications.Count == 0)
                {
                    return new GetSingleNotificationResponse(
                        StatusCodes.Status200OK,
                        "No notifications found",
                        new NotificationsSinglePayload
                        {
                            Items = new List<NotificationSingleItem>(),
                            TotalItems = totalItems,
                            HasNextPage = false
                        }
                    );
                }

                var processedItems = notifications.Select(notification =>
                {
                    var (title, message) = NotificationHelper.GetLocalizedContent(
                        user,
                        notification.TitleAr,
                        notification.TitleEn,
                        notification.MessageAr,
                        notification.MessageEn
                    );

                    return new NotificationSingleItem
                    {
                        Id = notification.Id,
                        Title = title,
                        Message = message,
                        CreatedAt = notification.CreatedAt
                    };
                }).ToList();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new NotificationsSinglePayload
                {
                    Items = processedItems,
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage,
                };

                return new GetSingleNotificationResponse(
                    StatusCodes.Status200OK,
                    "Notifications retrieved successfully",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving notifications for user {UserId}", user.Id);
                return new GetSingleNotificationResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }
    }
}
