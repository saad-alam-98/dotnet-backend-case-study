using SaudiPlus.Entities;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Entities.Notification;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class NotificationRepository : Repository<BaseNotification, int>, INotificationRepository
    {
        public NotificationRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<GroupNotification>> GetGroupNotifications()
        {
            return await _dbContext.Set<GroupNotification>()
                .Include(n => n.Users)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<GeneralNotification>> GetGeneralNotifications()
        {
            return await _dbContext.Set<GeneralNotification>()
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<IndividualNotification>> GetIndividualNotifications()
        {
            return await _dbContext.Set<IndividualNotification>()
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IndividualNotification?> GetIndividualNotificationById(int id)
        {
            return await 
                _dbContext.Set<IndividualNotification>()
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<GroupNotification?> GetGroupNotificationById(int id)
        {
            return 
                await _dbContext.Set<GroupNotification>()
                .Include(n => n.Users)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<GeneralNotification?> GetGeneralNotificationById(int id)
        {
            return 
                await _dbContext.Set<GeneralNotification>()
                .FirstOrDefaultAsync(n => n.Id == id);
        }
        public async Task<int> GetIndividualNotificationsCountForUser(Guid userId, string? searchKeyword)
        {
            var query = _dbContext.Set<IndividualNotification>()
                .Where(n => n.UserId == userId);

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                var keyword = searchKeyword.ToLower();
                query = query.Where(n =>
                    (n.TitleEn != null && n.TitleEn.ToLower().Contains(keyword)) ||
                    (n.TitleAr != null && n.TitleAr.ToLower().Contains(keyword)) ||
                    n.MessageEn.ToLower().Contains(keyword) ||
                    n.MessageAr.ToLower().Contains(keyword));
            }

            return await query.CountAsync();
        }

        public async Task<int> GetGroupNotificationsCountForUser(Guid userId, string? searchKeyword)
        {
            var query = _dbContext.Set<GroupNotification>()
                .Where(n => n.Users.Any(u => u.Id == userId));

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                var keyword = searchKeyword.ToLower();
                query = query.Where(n =>
                    (n.TitleEn != null && n.TitleEn.ToLower().Contains(keyword)) ||
                    (n.TitleAr != null && n.TitleAr.ToLower().Contains(keyword)) ||
                    n.MessageEn.ToLower().Contains(keyword) ||
                    n.MessageAr.ToLower().Contains(keyword));
            }

            return await query.CountAsync();
        }

        public async Task<int> GetGeneralNotificationsCountWithFilter(string? searchKeyword)
        {
            var query = _dbContext.Set<GeneralNotification>().AsQueryable();

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                var keyword = searchKeyword.ToLower();
                query = query.Where(n =>
                    (n.TitleEn != null && n.TitleEn.ToLower().Contains(keyword)) ||
                    (n.TitleAr != null && n.TitleAr.ToLower().Contains(keyword)) ||
                    n.MessageEn.ToLower().Contains(keyword) ||
                    n.MessageAr.ToLower().Contains(keyword));
            }

            return await query.CountAsync();
        }

        public async Task<(List<NotificationDto> notifications, int totalCount)> GetAllNotificationsForUserByDatePaginated(Guid userId, string? searchKeyword, int pageNumber, int pageSize)
        {
            var keyword = searchKeyword?.ToLower();

            Expression<Func<BaseNotification, bool>> searchCondition = n =>
                string.IsNullOrEmpty(keyword) || (
                    (n.TitleEn != null && n.TitleEn.ToLower().Contains(keyword)) ||
                    (n.TitleAr != null && n.TitleAr.ToLower().Contains(keyword)) ||
                    n.MessageEn.ToLower().Contains(keyword) ||
                    n.MessageAr.ToLower().Contains(keyword)
                );

            var combinedQuery =
                _dbContext.Set<IndividualNotification>()
                    .Where(n => n.UserId == userId)
                    .Where(searchCondition)
                    .Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        TitleAr = n.TitleAr,
                        TitleEn = n.TitleEn,
                        MessageAr = n.MessageAr,
                        MessageEn = n.MessageEn,
                        CreatedAt = n.CreatedAt
                    })
                .Union(
                    _dbContext.Set<GroupNotification>()
                        .Where(n => n.Users.Any(u => u.Id == userId))
                        .Where(searchCondition)
                        .Select(n => new NotificationDto
                        {
                            Id = n.Id,
                            TitleAr = n.TitleAr,
                            TitleEn = n.TitleEn,
                            MessageAr = n.MessageAr,
                            MessageEn = n.MessageEn,
                            CreatedAt = n.CreatedAt
                        })
                )
                .Union(
                    _dbContext.Set<GeneralNotification>()
                        .Where(searchCondition)
                        .Select(n => new NotificationDto
                        {
                            Id = n.Id,
                            TitleAr = n.TitleAr,
                            TitleEn = n.TitleEn,
                            MessageAr = n.MessageAr,
                            MessageEn = n.MessageEn,
                            CreatedAt = n.CreatedAt
                        })
                );

            var orderedQuery = combinedQuery.OrderByDescending(n => n.CreatedAt);

            var totalCount = await orderedQuery.CountAsync();

            var notifications = await orderedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (notifications, totalCount);
        }

    }
}