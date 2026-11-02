using AutoMapper;
using SaudiPlus.Enums;
using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Entities;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Bucket;
using SaudiPlus.Request.Status;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Response.Status;
using SaudiPlus.Entities.Identity;
using SaudiPlus.Interfaces.Service;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Service.Status
{
    public class StatusService : BaseService<StatusService>, IStatusService
    {
        private readonly BucketSetting _bucket;
        private readonly IFileService _fileService;
        private readonly SaudiPlusDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IOfferRepository _offerRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly IBusinessRepository _businessRepository;
        private readonly IStatusInteractionRepository _statusInteractionRepository;

        public StatusService(
            IMapper mapper,
            IFileService fileService,
            SaudiPlusDbContext dbContext,
            UserManager<User> userManager,
            ILogger<StatusService> logger,
            IOptions<BucketSetting> bucket,
            ILanguageService languageService,
            IOfferRepository offerRepository,
            IStatusRepository statusRepository,
            IBusinessRepository businessRepository,
            IStatusInteractionRepository statusInteractionRepository
        ) : base(mapper, logger, languageService)
        {
            _bucket = bucket.Value;
            _dbContext = dbContext;
            _userManager = userManager;
            _fileService = fileService;
            _offerRepository = offerRepository;
            _statusRepository = statusRepository;
            _businessRepository = businessRepository;
            _statusInteractionRepository = statusInteractionRepository;
        }

        public async Task<SuccessResponse> CreateStatus(CreateStatusDTO model, Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return new SuccessResponse(
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "Invalid User",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                var path = _bucket.Status + "/" + "Admin/" + model.Title;
                var isBusiness = await _userManager.IsInRoleAsync(user, UserRole.Business.ToString());
                Guid? businessId = null;
                if (isBusiness)
                {
                    var prefixParts = new List<string> { "Status" };

                    var business = await _businessRepository.GetBusinessByUserId(userId);
                    if (business != null)
                    {
                        prefixParts.Add(SanitizeForPath(business.NameEn));
                        businessId = business.Id;
                    }

                    if (model.OfferId != null)
                    {
                        var offer = await _offerRepository.GetByIdAsync(model.OfferId.Value);
                        if (offer != null)
                            prefixParts.Add(SanitizeForPath(offer.Title));
                    }

                    var prefix = string.Join("/", prefixParts);
                    var sanitizedTitle = SanitizeForPath(model.Title);
                    path = $"{_bucket.Status}/{prefix}/{sanitizedTitle}";
                }

                var uploadResponse = await _fileService.UploadFileAsync(
                    model.Media,
                    path,
                    20
                );

                var mediaType = Path.GetExtension(model.Media.FileName).ToLower();

                if (uploadResponse.error != null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: uploadResponse.error.Message,
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                var status = _mapper.Map<Entities.Status>(model);
                status.MediaType = mediaType;
                status.BusinessId = businessId;
                status.Duration = model.Duration ?? 86400;
                status.MediaUrl = uploadResponse.URL;
                status.IsActive = true;
                status.FromAdmin = !isBusiness;
                await _statusRepository.InsertAsync(status, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Status Created",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating status");
                return new SuccessResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> EditStatus(EditStatusDTO model, Guid userId)
        {
            var (languageCode, error) = _languageService.GetLanguageCode();
            if (error != null)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: error.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            var existingStatus = await _statusRepository.GetStatusWithRelation(model.Id);
            if (existingStatus == null)
            {
                return new SuccessResponse(
                    statusCode: StatusCodes.Status404NotFound,
                    message: $"Status with id {model.Id} not found",
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            try
            {
                (string URL, Exception? error) uploadResponse = ("", null);
                if (model.Media != null)
                {
                    var user = await _userManager.FindByIdAsync(userId.ToString());
                    if (user == null)
                    {
                        return new SuccessResponse(
                            statusCode: StatusCodes.Status500InternalServerError,
                            message: "Invalid User",
                            payload: new SuccessResponsePayload { success = false }
                        );
                    }

                    model.Title = string.IsNullOrEmpty(model.Title) ? existingStatus.Title : model.Title;
                    var path = _bucket.Status + "/" + "Admin/" + model.Title;

                    var isBusiness = await _userManager.IsInRoleAsync(user, UserRole.Business.ToString());
                    if (isBusiness)
                    {
                        var prefixParts = new List<string> { "Status" };

                        var business = await _businessRepository.GetBusinessByUserId(userId);
                        if (business != null)
                            prefixParts.Add(SanitizeForPath(business.NameEn));

                        if (model.OfferId != null)
                        {
                            var offer = await _offerRepository.GetByIdAsync(model.OfferId.Value);
                            if (offer != null)
                                prefixParts.Add(SanitizeForPath(offer.Title));
                        }

                        var prefix = string.Join("/", prefixParts);

                        var sanitizedTitle = SanitizeForPath(model.Title);
                        path = $"{_bucket.Status}/{prefix}/{sanitizedTitle}";
                    }

                    uploadResponse = await _fileService.UploadFileAsync(
                        model.Media,
                        path,
                        20
                    );

                    var mediaType = Path.GetExtension(model.Media.FileName).ToLower();
                    existingStatus.MediaType = mediaType;
                }

                if (!string.IsNullOrEmpty(uploadResponse.URL))
                {
                    existingStatus.MediaUrl = uploadResponse.URL;
                }

                model.IsActive = model.IsActive.HasValue ? model.IsActive.Value : existingStatus.IsActive;
                model.Duration = model.Duration.HasValue ? model.Duration : existingStatus.Duration;
               
                _mapper.Map(model, existingStatus);

                await _statusRepository.UpdateAsync(existingStatus, true);

                return new SuccessResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "Status Updated",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing status");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> ToggleStatus(ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _statusRepository.GetStatusWithRelation(model.Id);
                if (response == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Status with Id {model.Id} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                response.IsActive = model.IsActive;
                await _statusRepository.UpdateAsync(response, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Status",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating status status");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetStatusDetailResponse> GetStatusById(int id)
        {
            try
            {
                var response = await _statusRepository.GetStatusWithRelation(id);
                if (response == null)
                {
                    return new GetStatusDetailResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Status with Id {id} not found",
                        payload: null
                    );
                }

                var payload = _mapper.Map<GetStatusPayload>(response);
                payload.TotalInteractionTime = await GetTotalInteractionTimeForStatus(id);
                payload.TotalClicks = await GetTotalClicksForStatus(id);
                payload.TotalViews = await GetTotalViewsForStatus(id);

                return new GetStatusDetailResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Status",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting status");
                return new GetStatusDetailResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetStatusListResponse> GetBusinessStatus(Guid businessId, EntityFilter filter)
        {
            try
            {
                var business = await _businessRepository.GetByIdAsync(businessId);
                if (business == null)
                {
                    return new GetStatusListResponse(
                       statusCode: StatusCodes.Status500InternalServerError,
                       message: "Invalid Business User",
                       payload: null
                   );
                }
                var query = _statusRepository.GetBusinessStatus(business.Id);

                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(status => status.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(status =>
                        status.Title.ToLower().Contains(searchKeyword));
                }

                var totalItems = await query.CountAsync();
                var status = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new StatusCollection
                {
                    Items = [.. status.Select(b => {
                        var item = _mapper.Map<GetStatusPayload>(b);
                        return item;
                    })],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                foreach (var item in payload.Items)
                {
                    item.TotalInteractionTime = await GetTotalInteractionTimeForStatus(item.Id);
                    item.TotalClicks = await GetTotalClicksForStatus(item.Id);
                    item.TotalViews = await GetTotalViewsForStatus(item.Id);
                }

                return new GetStatusListResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Status Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching status");
                return new GetStatusListResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetStatusListResponse> GetStatusList(Guid userId, EntityFilter filter)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return new GetStatusListResponse(
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "Invalid User",
                        payload: null
                    );
                }

                IQueryable<Entities.Status>? query = null; 
                if (await _userManager.IsInRoleAsync(user, UserRole.Admin.ToString()))
                {
                    query = _statusRepository.GetAdminStatus();
                }
                else if (await _userManager.IsInRoleAsync(user, UserRole.Business.ToString()))
                {
                    var business = await _businessRepository.GetBusinessByUserId(userId);
                    if (business == null)
                    {
                        return new GetStatusListResponse(
                           statusCode: StatusCodes.Status500InternalServerError,
                           message: "Invalid Business User",
                           payload: null
                       );
                    }
                    query = _statusRepository.GetBusinessStatus(business.Id);
                }
                else
                {
                    return new GetStatusListResponse(
                        statusCode: StatusCodes.Status403Forbidden,
                        message: "Unauthorized role",
                        payload: null
                    );
                }

                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(status => status.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(status =>
                        status.Title.ToLower().Contains(searchKeyword));
                }

                var totalItems = await query.CountAsync();
                var status = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new StatusCollection
                {
                    Items = [.. status.Select(b => {
                        var item = _mapper.Map<GetStatusPayload>(b);
                        return item;
                    })],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                foreach (var item in payload.Items)
                {
                    item.TotalInteractionTime = await GetTotalInteractionTimeForStatus(item.Id);
                    item.TotalClicks = await GetTotalClicksForStatus(item.Id);
                    item.TotalViews = await GetTotalViewsForStatus(item.Id);
                }

                return new GetStatusListResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Status Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching status");
                return new GetStatusListResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetUserStatusListResponse> GetActiveStatus(Guid userId)
        {
            try
            {
                var query = _statusRepository.GetActiveStatus();
                var totalItems = await query.CountAsync();
                var status = await query.ToListAsync();

                var payload = new UserStatusCollection
                {
                    Items = [.. status.Select(b => {
                        var item = _mapper.Map<UserStatusPayload>(b);
                        return item;
                    })],
                    TotalItems = totalItems,
                    HasNextPage = false
                };

                foreach (var I in payload.Items)
                {
                    var interaction = await _statusInteractionRepository.GetByStatusAndUserAsync(I.Id, userId);
                    if (interaction != null)
                    {
                        I.IsClicked = interaction.IsClicked;
                        I.IsViewed = interaction.IsViewed;
                    }
                    I.ViewDuration = 30;
                }

                return new GetUserStatusListResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Status Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching status");
                return new GetUserStatusListResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> ViewStatus(int statusId, Guid userId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var status = await _statusRepository.GetByIdAsync(statusId);
                    if (status == null)
                        return new SuccessResponse(StatusCodes.Status400BadRequest, "Status not found", new() { success = false });

                    var interaction = await _statusInteractionRepository.GetByStatusAndUserAsync(statusId, userId);
                    if (interaction == null)
                    {
                        interaction = new StatusInteraction
                        {
                            StatusId = statusId,
                            IndividualUserId = userId,
                            IsViewed = true,
                            IsClicked = false,
                            IsActive = true
                        };

                        status.Views += 1;
                        await _statusInteractionRepository.InsertAsync(interaction, true);
                    }
                    else if (!interaction.IsViewed)
                    {
                        interaction.IsViewed = true;

                        status.Views += 1;
                        await _statusInteractionRepository.UpdateAsync(interaction, true);
                    }

                    await _statusRepository.UpdateAsync(status, true);
                    await transaction.CommitAsync();

                    return new SuccessResponse(StatusCodes.Status200OK, "Status Viewed", new() { success = true });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error viewing status");
                    return new SuccessResponse(StatusCodes.Status500InternalServerError, "Unexpected error", new() { success = false });
                }
            });
        }

        public async Task<SuccessResponse> RecordClick(int statusId, Guid userId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var status = await _statusRepository.GetByIdAsync(statusId);
                    if (status == null)
                        return new SuccessResponse(StatusCodes.Status400BadRequest, "Status not found", new() { success = false });

                    var interaction = await _statusInteractionRepository.GetByStatusAndUserAsync(statusId, userId);
                    if (interaction == null)
                    {
                        interaction = new StatusInteraction
                        {
                            StatusId = statusId,
                            IndividualUserId = userId,
                            IsViewed = false,
                            IsClicked = true,
                            IsActive = true
                        };

                        status.Clicks += 1;
                        await _statusInteractionRepository.InsertAsync(interaction, true);
                    }
                    else if (!interaction.IsClicked)
                    {
                        interaction.IsClicked = true;

                        status.Clicks += 1;
                        await _statusInteractionRepository.UpdateAsync(interaction, true);
                    }

                    await _statusRepository.UpdateAsync(status, true);
                    await transaction.CommitAsync();

                    return new SuccessResponse(StatusCodes.Status200OK, "Click Recorded", new() { success = true });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error recording click");
                    return new SuccessResponse(StatusCodes.Status500InternalServerError, "Unexpected error", new() { success = false });
                }
            });
        }

        public async Task<SuccessResponse> RecordInteractionTime(int statusId, Guid userId, int seconds)
        {
            try
            {
                var status = await _statusRepository.GetByIdAsync(statusId);
                if (status == null)
                {
                    return new SuccessResponse(StatusCodes.Status400BadRequest, "Status not found", new() { success = false });
                }

                var interactionTime = Math.Min(seconds, 30);
                var interaction = await _statusInteractionRepository.GetByStatusAndUserAsync(statusId, userId);

                if (interaction == null)
                {
                    interaction = new StatusInteraction
                    {
                        StatusId = statusId,
                        IndividualUserId = userId,
                        IsViewed = false,
                        IsClicked = false,
                        InteractionTime = interactionTime
                    };

                    await _statusInteractionRepository.InsertAsync(interaction, true);
                }
                else
                {
                    if (interaction.InteractionTime == null || interactionTime > interaction.InteractionTime)
                    {
                        interaction.InteractionTime = interactionTime;
                        await _statusInteractionRepository.UpdateAsync(interaction, true);
                    }
                }

                return new SuccessResponse(
                    statusCode: StatusCodes.Status200OK, 
                    message: "Interaction time recorded",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording interaction time");
                return new SuccessResponse(StatusCodes.Status500InternalServerError, "Unexpected error", new() { success = false });
            }
        }

        public async Task<SuccessResponse> DeleteStatus(int id)
        {
            try
            {
                var banner = await _statusRepository.GetByIdAsync(id);
                if (banner == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status404NotFound,
                       message: $"Banner with Id {id} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                if (!string.IsNullOrEmpty(banner.MediaUrl))
                {
                    await _fileService.DeleteFileAsync(banner.MediaUrl);
                }

                await _statusRepository.DeleteAsync(id, true);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Status Deleted",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting status");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        private string SanitizeForPath(string input)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                input = input.Replace(c, '_');

            return input.Replace(" ", "_").Trim();
        }

        public async Task<int> GetTotalInteractionTimeForStatus(int statusId)
        {
            return (await _statusInteractionRepository.GetStatusInteractions(statusId))
                .Where(x => x.InteractionTime != null)
                .Sum(x => x.InteractionTime!.Value);
        }

        public async Task<int> GetTotalClicksForStatus(int statusId)
        {
            return (await _statusInteractionRepository.GetStatusInteractions(statusId))
                .Where(x => x.IsClicked)
                .Count();
        }

        public async Task<int> GetTotalViewsForStatus(int statusId)
        {
            return (await _statusInteractionRepository.GetStatusInteractions(statusId))
                .Where(x => x.IsViewed)
                .Count();
        }
    }
}
