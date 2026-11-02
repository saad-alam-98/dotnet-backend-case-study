using AutoMapper;
using SaudiPlus.Enums;
using SaudiPlus.Response;
using SaudiPlus.Entities;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Invoice;
using SaudiPlus.Response.Invoice;
using SaudiPlus.Interfaces.Service;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Request.Notification;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Service.Invoice
{
    public class InvoiceService : BaseService<InvoiceService>, IInvoiceService
    {
        private readonly SaudiPlusDbContext _dbContext;
        private readonly IOfferRepository _offerRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUserOfferTokenRepository _userOfferTokenRepository;
        private readonly INotificationCenterService _notificationCenterService;
        private readonly IIndividualUserRepository _individualUserRepository;

        public InvoiceService(
            IMapper mapper,
            SaudiPlusDbContext dbContext,
            ILogger<InvoiceService> logger,
            IOfferRepository offerRepository,
            ILanguageService languageService,
            IBranchRepository branchRepository,
            IInvoiceRepository invoiceRepository,
            IIndividualUserRepository individualUserRepository,
            IUserOfferTokenRepository userOfferTokenRepository,
            INotificationCenterService notificationCenterService
        ) : base(mapper, logger, languageService)
        {
            _dbContext = dbContext;
            _offerRepository = offerRepository;
            _branchRepository = branchRepository;
            _invoiceRepository = invoiceRepository;
            _notificationCenterService = notificationCenterService;
            _userOfferTokenRepository = userOfferTokenRepository;
            _individualUserRepository = individualUserRepository;
        }

        public async Task<CreateInvoiceResponse> CreateInvoice(Entities.BusinessUser businessUser, CreateInvoiceDTO model)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var (languageCode, languageError) = _languageService.GetLanguageCode();
                    if (languageError != null)
                    {
                        return new CreateInvoiceResponse
                        (
                            statusCode: StatusCodes.Status400BadRequest,
                            message: languageError.Message,
                            payload: null
                        );
                    }

                    var offer = await _offerRepository.GetByIdAsync(model.OfferId);
                    if (offer == null)
                    {
                        await transaction.RollbackAsync();
                        return new CreateInvoiceResponse(
                            statusCode: StatusCodes.Status400BadRequest,
                            message: $"Offer with Id {model.OfferId} not found",
                            payload: null
                        );
                    }

                    Entities.IndividualUser? user = null;
                    if (model.MobileNumber.Length < 6)
                    {
                        var token = await _userOfferTokenRepository.GetUserTokenWithRelation(model.MobileNumber);
                        if (token != null)
                        {
                            user = token.User;
                            if (token.BranchId != model.BranchId)
                            {
                                await transaction.RollbackAsync();
                                return new CreateInvoiceResponse(
                                    statusCode: StatusCodes.Status400BadRequest,
                                    message: $"Invalid redeem code",
                                    payload: null
                                );
                            }
                            else if (token.ExpiryTime < DateTime.UtcNow)
                            {
                                await transaction.RollbackAsync();
                                return new CreateInvoiceResponse(
                                    statusCode: StatusCodes.Status400BadRequest,
                                    message: $"Redeem code has expired",
                                    payload: null
                                );
                            }

                            await _userOfferTokenRepository.DeleteAsync(token.Id, true);
                        }
                    }
                    else
                    {
                        user = await _individualUserRepository.GetUserByMobileNumber(model.MobileNumber);
                    }

                    if (user == null)
                    {
                        await transaction.RollbackAsync();
                        return new CreateInvoiceResponse(
                            statusCode: StatusCodes.Status400BadRequest,
                            message: $"Invalid user mobile number or token",
                            payload: null
                        );
                    }

                    if (offer.BusinessId != businessUser.Id)
                    {
                        await transaction.RollbackAsync();
                        return new CreateInvoiceResponse(
                            statusCode: StatusCodes.Status400BadRequest,
                            message: $"This offer is not associated with given business",
                            payload: null
                        );
                    }

                    var branch = (await _branchRepository.GetBusinessBranches(businessUser.Id))
                            .FirstOrDefault(b => b.Id == model.BranchId);

                    if (branch == null)
                    {
                        await transaction.RollbackAsync();
                        return new CreateInvoiceResponse(
                            statusCode: StatusCodes.Status400BadRequest,
                            message: $"Invalid branch",
                            payload: null
                        );
                    }

                    var totalOffers = (await _offerRepository.GetBusinessOffers(businessUser.Id, false)).Sum(b => b.RedeemCount);
                    totalOffers += 1;

                    var invoice = new Entities.Invoice
                    {
                        UserId = user.Id,
                        BusinessId = businessUser.Id,
                        OfferId = offer.Id,
                        Amount = model.Amount,
                        IssueDate = DateTime.UtcNow,
                        InvoiceNumber = $"#INV-{totalOffers}",
                        BranchId = branch.Id
                    };

                    float discountValue = 0;
                    if (offer.OfferType == OfferType.Percentage)
                    {
                        var percentage = offer.PercentageValue ?? 0;
                        discountValue = (model.Amount / 100) * percentage;
                    }
                    else if (offer.OfferType == OfferType.Invoice)
                    {
                        var discount = offer.InvoiceValue ?? 0;
                        discountValue = discount;
                    }
                    invoice.Discount = (int)discountValue;
                    invoice.TotalAmount = Math.Max(0, model.Amount - discountValue);

                    offer.RedeemCount += 1;
                    await _offerRepository.UpdateAsync(offer, true);

                    invoice.IsActive = true;
                    invoice.IsVerified = false;

                    await _invoiceRepository.InsertAsync(invoice, true);
                    var payload = _mapper.Map<InvoiceDetailPayload>(invoice);
                    payload.SetBusinessNameLocalized(invoice.Business.GetNameByLanguageCode(languageCode));
                    payload.InvoiceId = invoice.Id;

                    await transaction.CommitAsync();

                    return new CreateInvoiceResponse(
                        StatusCodes.Status200OK,
                        "Invoice created successfully.",
                        payload
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new CreateInvoiceResponse(
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "An unexpected error occurred. Please try again later.",
                        payload: null
                    );
                }
            });
        }

        public async Task<GetInvoicesResponse> GetInvoiceByFilter(Guid businessUserId, EntityFilter filter)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetInvoicesResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var query = _invoiceRepository.GetInvoiceForFilter(businessUserId);
                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(invoice => invoice.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(invoice =>
                        invoice.InvoiceNumber.ToLower()
                        .Contains(searchKeyword)
                    );
                }

                var totalItems = await query.CountAsync();
                var invoices = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                .Take(filter.pageSize)
                .ToListAsync();
                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new GetInvoicesPayload
                {
                    Items = invoices.Select(b =>
                    {
                        var item = _mapper.Map<InvoiceDetailPayload>(b);
                        item.SetBusinessNameLocalized(b.Business.GetNameByLanguageCode(languageCode));
                        return item;
                    }).ToList(),
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                return new GetInvoicesResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Users Fetch Success",
                    payload: payload
                );
            }
            catch
            {
                return new GetInvoicesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }


        public async Task<GetOfferForInvoiceResponse> GetBusinessInvoiceOffers(Guid businessUserId)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetOfferForInvoiceResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var query = await _offerRepository.GetBusinessOffers(businessUserId, true);
                var offers = query.Where(q => q.RedeemByCode == false);

                var payload = new GetOfferForInvoicePayload
                {
                    Items = []
                };

                foreach (var item in offers)
                {
                    payload.Items.Add(new GetOfferForInvoiceItem
                    {
                        Id = item.Id,
                        Name = item.Title
                    });
                }

                return new GetOfferForInvoiceResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Users Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                return new GetOfferForInvoiceResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBranchForInvoice> GetBranchForInvoice(Guid businessUserId)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetBranchForInvoice
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var branches = await _branchRepository.GetBusinessBranches(businessUserId);

                var payload = new BranchForInvoicePayload
                {
                    Items = []
                };

                foreach (var branch in branches)
                {
                    payload.Items.Add(_mapper.Map<InvoiceBranchDetail>(branch));
                }

                return new GetBranchForInvoice
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Get Branch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                return new GetBranchForInvoice
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> SaveInvoice(Entities.BusinessUser businessUser, SaveInvoiceDTO model)
        {
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceWithUserRelation(model.InvoiceId);
                if (invoice == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Invalid invoice id",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                invoice.User.TotalPoints += invoice.Offer.Points;
                await _individualUserRepository.UpdateAsync(invoice.User, true);

                invoice.IsVerified = true;
                await _invoiceRepository.UpdateAsync(invoice, true);

                await _notificationCenterService.SendIndividualNotificationForget(
                    new IndividualNotificationRequestDTO
                    {
                        Email = invoice.User.Email,
                        TitleEn = "Offer Redeemed Successfully!",
                        TitleAr = "تم استرداد العرض بنجاح",
                        MessageEn = "Congratulations! You have successfully redeemed your offer.",
                        MessageAr = "تهانينا! لقد قمت باسترداد عرضك بنجاح. تحقق من مكافآتك الآن"
                    }
                );

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Invoice Saved",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving invoice");
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
