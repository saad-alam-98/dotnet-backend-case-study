using AutoMapper;
using SaudiPlus.Enums;
using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Entities;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Support;
using SaudiPlus.Response.Support;
using SaudiPlus.Entities.Identity;
using SaudiPlus.Interfaces.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;
using SaudiPlus.Request.Notification;

namespace SaudiPlus.Service.Support
{
    public class SupportService : BaseService<SupportService>, ISupportService
    {
        private readonly SaudiPlusDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly ISupportTypeRepository _supportTypeRepository;
        private readonly ISupportTicketRepository _supportTicketRepository;
        private readonly ISupportMessageRepository _supportMessageRepository;
        private readonly INotificationCenterService _notificationCenterService;


        public SupportService(
            SaudiPlusDbContext dbContext,
            UserManager<User> userManager,
            ISupportTypeRepository supportTypeRepository,
            ISupportTicketRepository supportTicketRepository,
            ISupportMessageRepository supportMessageRepository,
            INotificationCenterService notificationCenterService,
            ILanguageService languageService,
            ILogger<SupportService> logger,
            IMapper mapper
        ) : base(mapper, logger, languageService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _supportTypeRepository = supportTypeRepository;
            _supportTicketRepository = supportTicketRepository;
            _supportMessageRepository = supportMessageRepository;
            _notificationCenterService = notificationCenterService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<SuccessResponse> CreateSupportType(AddSupportTypeDTO model)
        {
            try
            {
                var supportType = _mapper.Map<SupportType>(model);
                supportType.IsActive = true;
                await _supportTypeRepository.InsertAsync(supportType, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Support Type Created",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }
        
        public async Task<SuccessResponse> EditSupportType(EditSupportTypeDTO model)
        {
            var existingSupport = await _supportTypeRepository.GetByIdAsync(model.Id);
            if (existingSupport == null)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: $"Support Type with Id {model.Id} not found",
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            try
            {
                model.IsActive = model.IsActive.HasValue ? model.IsActive.Value : existingSupport.IsActive;
                _mapper.Map(model, existingSupport);

                await _supportTypeRepository.UpdateAsync(existingSupport, true);

                return new SuccessResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "Support type updated",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }
        
        public async Task<SuccessResponse> ToggleSupportType(ToggleStatusDTO<int> model)
        {
            try
            {
                var existingSupport = await _supportTypeRepository.GetByIdAsync(model.Id);
                if (existingSupport == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Support Type with Id {model.Id} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                existingSupport.IsActive = model.IsActive;
                await _supportTypeRepository.UpdateAsync(existingSupport, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Success",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }
        
        public async Task<GetSupportTypes> GetSupportTypes(EntityFilter filter)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetSupportTypes
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var query = _supportTypeRepository.GetSupportTypeForFilter();
                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(supportType => supportType.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(supportType =>
                        supportType.NameEn.ToLower().Contains(searchKeyword) ||
                        supportType.NameAr.ToLower().Contains(searchKeyword)
                    );
                }

                var totalItems = await query.CountAsync();
                var supportTypes = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                .Take(filter.pageSize)
                .ToListAsync();
                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new GetSupportTypePayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                foreach (var supportType in supportTypes)
                {
                    var item = _mapper.Map<SupportTypeItemDetailed>(supportType);
                    payload.Items.Add(item);
                }

                return new GetSupportTypes
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Users Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching user");
                return new GetSupportTypes
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }
        
        public async Task<GetSupportTypes> GetActiveSupportTypes()
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetSupportTypes
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var response = (await _supportTypeRepository.GetAsync()).Where(s => s.IsActive);

                var payload = new GetSupportTypePayload()
                {
                    Items = [],
                    TotalItems = response.Count(),
                    HasNextPage = false
                };
                foreach (var r in response)
                {
                    var item = _mapper.Map<SupportTypeItemLocalized>(r);
                    item.SetSupportTypeLocalized(r.GetNameByLanguageCode(languageCode));
                    payload.Items.Add(item);
                }

                return new GetSupportTypes
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Active Support Types",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                return new GetSupportTypes
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<CreateTicketResponse> CreateSupportTicket(Entities.IndividualUser user, CreateSupportTicketDTO model)
        {
            var (languageCode, languageError) = _languageService.GetLanguageCode();
            if (languageError != null)
            {
                return new CreateTicketResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: languageError.Message,
                    payload: null
                );
            }

            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var hasOpenTicket = await _supportTicketRepository.HasOpenTickets(user.Id);

                    if (hasOpenTicket)
                    {
                        await transaction.RollbackAsync();
                        return new CreateTicketResponse(
                            StatusCodes.Status400BadRequest,
                            "You already have an open support ticket. Please wait for it to be resolved before creating a new one.",
                            null
                        );
                    }

                    var supportType = await _supportTypeRepository.FindAsync(model.SupportTypeId);
                    if (supportType == null)
                    {
                        await transaction.RollbackAsync();
                        return new CreateTicketResponse(
                            StatusCodes.Status400BadRequest,
                            "Invalid support type ID.",
                            null
                        );
                    }

                    var supportTicket = _mapper.Map<SupportTicket>(model);
                    supportTicket.UserId = user.Id;
                    supportTicket.TicketStatus = TicketStatus.New;
                    supportTicket.IsActive = true;

                    await _supportTicketRepository.InsertAsync(supportTicket, autoSave: true);

                    var savedTicket = await _supportTicketRepository.GetSupportTicketWithRelation(supportTicket.Id);

                    var supportMessage = new SupportMessage
                    {
                        TicketId = savedTicket.Id,
                        UserId = user.Id,
                        IsAdmin = false,
                        Message = model.Message,
                        SentAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _supportMessageRepository.InsertAsync(supportMessage, autoSave: true);
                    var payload = _mapper.Map<GetTicketPayload>(savedTicket);
                    payload.Message = model.Message;
                    payload.SupportType.SetSupportTypeLocalized(savedTicket.SupportType.GetNameByLanguageCode(languageCode));

                    await transaction.CommitAsync();

                    return new CreateTicketResponse(
                        StatusCodes.Status200OK,
                        "Support ticket created successfully.",
                        payload
                    );
                }
                catch(Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new CreateTicketResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "An unexpected error occurred. Please try again later.",
                        payload: null
                    );
                }
            });
        }

        public async Task<SuccessResponse> CloseSupportTicket(int ticketId)
        {
            try
            {
                var ticket = await _supportTicketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Support Ticket not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                if (ticket.TicketStatus == TicketStatus.Close)
                {
                    return new SuccessResponse
                   (
                       statusCode: StatusCodes.Status400BadRequest,
                       message: $"Support Ticket {ticketId} already closed",
                       payload: new SuccessResponsePayload { success = false }
                   );
                }

                ticket.IsActive = false;
                ticket.TicketStatus = TicketStatus.Close;
                await _supportTicketRepository.UpdateAsync(ticket, true);
                
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Support Ticket Closed",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }
        public async Task<SuccessResponse> DeleteSupportTicket(int ticketId)
        {
            try
            {
                var ticket = await _supportTicketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Support Ticket not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                await _supportTicketRepository.DeleteAsync(ticketId, true);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Support Ticket Deleted",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }
        public async Task<SuccessResponse> DeleteSupportType(int typeId)
        {
            try
            {
                var supportType = await _supportTypeRepository.GetByIdAsync(typeId);
                if (supportType == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Support Type with Id {typeId} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                var hasRelatedTickets = await _supportTicketRepository.HasRelatedTickets(typeId);

                if (hasRelatedTickets)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Cannot delete this support type because it is being used by existing tickets. Please close or reassign those tickets first.",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                await _supportTypeRepository.DeleteAsync(typeId, true);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Support Type Deleted",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetTicketsResponse> GetOpenTickets(EntityFilter filter)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetTicketsResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var (tickets, totalItems) = await _supportTicketRepository.GetSupportTicketWithRelationPaginated(
                    pageSize: filter.pageSize,
                    pageNumber: filter.pageNumber,
                    searchKeyword: filter.searchKeyword,
                    isActive: true,
                    statuses: new[] { TicketStatus.New, TicketStatus.Open }
                );

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new GetTicketsPayloadPaginated()
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage,
                };

                foreach (var ticket in tickets)
                {
                    var item = _mapper.Map<GetTicketItemPayload>(ticket);
                    item.SupportType.SetSupportTypeLocalized(ticket.SupportType.GetNameByLanguageCode(languageCode));
                    payload.Items.Add(item);
                }

                return new GetTicketsResponse(
                    StatusCodes.Status200OK,
                    "Get Open Tickets Success",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching open tickets");
                return new GetTicketsResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<GetTicketsResponse> GetClosedTickets(EntityFilter filter)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetTicketsResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var (tickets, totalItems) = await _supportTicketRepository.GetSupportTicketWithRelationPaginated(
                    pageSize: filter.pageSize,
                    pageNumber: filter.pageNumber,
                    searchKeyword: filter.searchKeyword,
                    isActive: false,
                    statuses: new[] { TicketStatus.Close }
                );

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new GetTicketsPayloadPaginated()
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage,
                };

                foreach (var ticket in tickets)
                {
                    var item = _mapper.Map<GetTicketItemPayload>(ticket);
                    item.SupportType.SetSupportTypeLocalized(ticket.SupportType.GetNameByLanguageCode(languageCode));
                    payload.Items.Add(item);
                }

                return new GetTicketsResponse(
                    StatusCodes.Status200OK,
                    "Get Closed Tickets Success",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching closed tickets");
                return new GetTicketsResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<GetTicketsResponse> GetCurrentTicket(Entities.IndividualUser user)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetTicketsResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var (tickets, totalItems) = await _supportTicketRepository.GetUserTicketsPaginated(
                    userId: user.Id,
                    pageSize: 10,
                    pageNumber: 1,
                    isActive: true,
                    status: null
                );

                var payload = new GetTicketsPayload()
                {
                    Items = []
                };

                foreach (var ticket in tickets)
                {
                    var item = _mapper.Map<GetTicketItemPayload>(ticket);
                    item.SupportType.SetSupportTypeLocalized(ticket.SupportType.GetNameByLanguageCode(languageCode));
                    payload.Items.Add(item);
                }

                return new GetTicketsResponse(
                    StatusCodes.Status200OK,
                    "Get User Tickets Success",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching user tickets");
                return new GetTicketsResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<SuccessResponse> ReplyTicket(bool isAdmin, ReplyTicketDTO model)
        {
            try
            {
                var existingTicket = await _supportTicketRepository.GetByIdAsync(model.TicketId);
                if (existingTicket == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Support Ticket with Id {model.TicketId} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                if (!existingTicket.IsActive || existingTicket.TicketStatus == TicketStatus.Close)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Cannot reply to a closed ticket",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                if (existingTicket.TicketStatus == TicketStatus.New && isAdmin)
                {
                    existingTicket.TicketStatus = TicketStatus.Open;
                    await _supportTicketRepository.UpdateAsync(existingTicket, true);
                }

                if(isAdmin)
                {
                    var user = await _userManager.FindByIdAsync(existingTicket.UserId.ToString());
                    if (user != null)
                    {
                        await _notificationCenterService.SendIndividualNotificationForget(
                         new IndividualNotificationRequestDTO
                          {
                             Email = user.Email,
                              TitleEn = "New Reply to Your Support Ticket",
                              TitleAr = "رد جديد على تذكرة الدعم الخاصة بك",
                              MessageEn = $"Your support ticket has received a new reply: {model.Message}",
                              MessageAr = $"تلقت تذكرة الدعم الخاصة بك ردًا جديدًا: {model.Message}"
                          }
                        );
                    }
                }

                var supportMessage = new SupportMessage
                {
                    TicketId = existingTicket.Id,
                    UserId = existingTicket.UserId,
                    IsAdmin = isAdmin,
                    Message = model.Message,
                    SentAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _supportMessageRepository.InsertAsync(supportMessage, autoSave: true);

                return new SuccessResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "Success",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }
    }
}
