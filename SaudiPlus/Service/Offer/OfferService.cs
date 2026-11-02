using AutoMapper;
using SaudiPlus.Enums;
using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Entities;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Bucket;
using SaudiPlus.Request.Offer;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Response.Offer;
using SaudiPlus.Helper.Location;
using SaudiPlus.Request.Category;
using SaudiPlus.Response.Category;
using SaudiPlus.Entities.Identity;
using Microsoft.Extensions.Options;
using SaudiPlus.Interfaces.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SaudiPlus.Response.BusinessUser;
using SaudiPlus.Interfaces.Repository;
using SaudiPlus.Repository;

namespace SaudiPlus.Service.Offer
{
    public class OfferService : BaseService<OfferService>, IOfferService
    {
        private readonly BucketSetting _bucket;
        private readonly IMapService _mapService;
        private readonly IFileService _fileService;
        private readonly SaudiPlusDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IOfferRepository _offerRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBusinessRepository _businessUserRepository;
        private readonly IIndividualUserRepository _individualUserRepository;
        private readonly IUserOfferTokenRepository _userOfferTokenRepository;
        private readonly IUserOfferRedeemRepository _userOfferRedeemRepository;

        public OfferService(
            IMapService mapService,
            IFileService fileService,
            ILogger<OfferService> logger,
            SaudiPlusDbContext dbContext,
            UserManager<User> userManager,
            IOptions<BucketSetting> bucket,
            ILanguageService languageService,
            IOfferRepository offerRepository,
            IBranchRepository branchRepository,
            ICategoryRepository categoryRepository,
            IBusinessRepository businessUserRepository,
            IIndividualUserRepository individualUserRepository,
            IUserOfferTokenRepository userOfferTokenRepository,
            IUserOfferRedeemRepository userOfferRedeemRepository,
            IMapper mapper
        ) : base(mapper, logger, languageService)
        {
            _dbContext = dbContext;
            _bucket = bucket.Value;
            _mapService = mapService;
            _fileService = fileService;
            _userManager = userManager;
            _offerRepository = offerRepository;
            _branchRepository = branchRepository;
            _categoryRepository = categoryRepository;
            _businessUserRepository = businessUserRepository;
            _individualUserRepository = individualUserRepository;
            _userOfferTokenRepository = userOfferTokenRepository;
            _userOfferRedeemRepository = userOfferRedeemRepository;
        }

        public async Task<GetOfferResponse> CreateOffer(Guid BusinessId, IOfferDTO model)
        {
            if (model.Logos.Count > 5)
            {
                return new GetOfferResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "You can upload a maximum of 5 logos.",
                    payload: null
                );
            }

            var (languageCode, languageError) = _languageService.GetLanguageCode();
            if (languageError != null)
            {
                return new GetOfferResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: languageError.Message,
                    payload: null
                );
            }

            var validateOffer = ValidateOfferType(model.OfferType, model.InvoiceValue, model.PercentageValue);
            if (validateOffer != null)
                return validateOffer;

            var validateCode = ValidateRedeemByCode(model.RedeemByCode, model.RedeemCode);
            if (validateCode != null)
                return validateCode;

            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var offer = _mapper.Map<Entities.Offer>(model);
                    offer.BusinessId = BusinessId;
                    offer.IsActive = true;
                    await _offerRepository.InsertAsync(offer, true);
                    var entry = await _offerRepository.GetOfferWithRelations(offer.Id);

                    if (entry != null)
                    {
                        for (var index = 0; index < model.Logos.Count; index++)
                        {
                            var file = model.Logos[index];
                            var uploadResponse = await _fileService.UploadFileAsync(
                                file,
                               $"{_bucket.Business}/{_bucket.Offer}/{model.Title}/{index + 1}"
                            );

                            if (uploadResponse.error != null)
                            {
                                await transaction.RollbackAsync();
                                return new GetOfferResponse
                                (
                                    statusCode: StatusCodes.Status400BadRequest,
                                    message: uploadResponse.error.Message,
                                    payload: null
                                );
                            }

                            var offerLogo = new OfferLogo
                            {
                                OfferId = offer.Id,
                                LogoUrl = uploadResponse.URL,
                                IsActive = true
                            };

                            entry.Logos.Add(offerLogo);
                        }

                        await _offerRepository.UpdateAsync(entry, true);
                    }

                    var payload = _mapper.Map<OfferPayloadItem>(entry);
                    payload.OfferUrl = [];
                    foreach (var logo in entry.Logos)
                    {
                        payload.OfferUrl.Add(logo.LogoUrl);
                    }

                    payload.SetBusinessNameLocalized(entry!.Business.GetNameByLanguageCode(languageCode));
                    payload.Business.SetCategoryNameLocalized(entry!.Business.Category.GetNameByLanguageCode(languageCode));

                    await transaction.CommitAsync();

                    return new GetOfferResponse
                    (
                        statusCode: StatusCodes.Status200OK,
                        message: "Offer Created",
                        payload: payload
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while creating business offers");
                    return new GetOfferResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "An unexpected error occurred. Please try again later.",
                        payload: null
                    );
                }
            });
        }

        public async Task<GetOfferResponse> EditOffer(EditOfferDTO model)
        {
            if (model.Logos != null && model.Logos.Count > 5)
            {
                return new GetOfferResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "You can upload a maximum of 5 logos.",
                    payload: null
                );
            }

            var (languageCode, languageError) = _languageService.GetLanguageCode();
            if (languageError != null)
            {
                return new GetOfferResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: languageError.Message,
                    payload: null
                );
            }

            var existingOffer = await _offerRepository.GetOfferWithRelations(model.Id);
            if (existingOffer == null)
            {
                return new GetOfferResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: $"Offer with Id {model.Id} not found",
                    payload: null
                );
            }

            if (model.OfferType.HasValue)
            {
                var validateOffer = ValidateOfferType(model.OfferType.Value, model.InvoiceValue, model.PercentageValue);
                if (validateOffer != null)
                    return validateOffer;
            }

            if (model.RedeemByCode.HasValue)
            {
                var validateCode = ValidateRedeemByCode(model.RedeemByCode.Value, model.RedeemCode);
                if (validateCode != null)
                    return validateCode;
            }

            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    if (model.Logos != null)
                    {
                        foreach (var logo in existingOffer.Logos)
                        {
                            var deleteResponse = await _fileService.DeleteFileAsync(logo.LogoUrl);
                            if (deleteResponse.error != null)
                            {
                                await transaction.RollbackAsync();
                                return new GetOfferResponse
                                (
                                    statusCode: StatusCodes.Status400BadRequest,
                                    message: deleteResponse.error.Message,
                                    payload: null
                                );
                            }
                        }
                        existingOffer.Logos.Clear();

                        for (var index = 0; index < model.Logos.Count; index++)
                        {
                            var file = model.Logos[index];
                            var uploadResponse = await _fileService.UploadFileAsync(
                                file,
                                $"{_bucket.Business}/{_bucket.Offer}/{model.Title}/{index + 1}"
                            );

                            if (uploadResponse.error != null)
                            {
                                await transaction.RollbackAsync();
                                return new GetOfferResponse
                                (
                                    statusCode: StatusCodes.Status400BadRequest,
                                    message: uploadResponse.error.Message,
                                    payload: null
                                );
                            }

                            var offerLogo = new OfferLogo
                            {
                                OfferId = existingOffer.Id,
                                LogoUrl = uploadResponse.URL,
                                IsActive = true
                            };

                            existingOffer.Logos.Add(offerLogo);
                        }
                    }

                    existingOffer.PercentageValue = null;
                    existingOffer.InvoiceValue = null;

                    model.IsActive = model.IsActive ?? existingOffer.IsActive;
                    _mapper.Map(model, existingOffer);

                    await _offerRepository.UpdateAsync(existingOffer, true);
                    var payload = _mapper.Map<OfferPayloadItem>(existingOffer);
                    payload.OfferUrl = new();
                    foreach (var logo in existingOffer.Logos)
                    {
                        payload.OfferUrl.Add(logo.LogoUrl);
                    }

                    payload.SetBusinessNameLocalized(existingOffer.Business.GetNameByLanguageCode(languageCode));
                    payload.Business.SetCategoryNameLocalized(existingOffer.Business.Category.GetNameByLanguageCode(languageCode));

                    await transaction.CommitAsync();
                    return new GetOfferResponse(
                        statusCode: StatusCodes.Status200OK,
                        message: "Offer Updated",
                        payload: payload
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while editing business offers");
                    return new GetOfferResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "An unexpected error occurred. Please try again later.",
                        payload: null
                    );
                }
            });
        }

        public async Task<GetOfferResponse> GetOfferById(int offerId, LocationDTO? location)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetOfferResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var offer = await _offerRepository.GetOfferWithRelations(offerId);
                if (offer == null)
                {
                    return new GetOfferResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Offer with Id {offerId} not found",
                        payload: null
                    );
                }
                var payload = _mapper.Map<OfferPayloadItem>(offer);
                if (location != null)
                {
                    payload.KmAway = (offer.Business.GetNearestBranchWithDistance(location.Latitude, location.Longitude)).Distance;
                }
                payload.OfferUrl = [];
                foreach (var logo in offer.Logos)
                {
                    payload.OfferUrl.Add(logo.LogoUrl);
                }

                payload.SetBusinessNameLocalized(offer.Business.GetNameByLanguageCode(languageCode));
                payload.Business.SetCategoryNameLocalized(offer.Business.Category.GetNameByLanguageCode(languageCode));

                return new GetOfferResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while getting offer with id {offerId}");
                return new GetOfferResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> ToggleOfferStatus(ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _offerRepository.GetOfferWithRelations(model.Id);
                if (response == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Offer with Id {model.Id} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                response.IsActive = model.IsActive;
                await _offerRepository.UpdateAsync(response, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Offers Status Updated",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating offer status");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> DeleteOffer(int offerId)
        {
            try
            {
                var offer = await _offerRepository.GetByIdAsync(offerId);
                if (offer == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Offer with Id {offerId} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                foreach (var logo in offer.Logos)
                {
                    var deleteResponse = await _fileService.DeleteFileAsync(logo.LogoUrl);
                    if (deleteResponse.error != null)
                    {
                        return new SuccessResponse
                        (
                            statusCode: StatusCodes.Status400BadRequest,
                            message: deleteResponse.error.Message,
                            payload: new SuccessResponsePayload { success = false }
                        );
                    }
                }

                await _offerRepository.DeleteAsync(offerId, true);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Offer Deleted",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting offer");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        private GetOfferResponse? ValidateOfferType(
            OfferType OfferType,
            decimal? InvoiceValue,
            decimal? PercentageValue
        )
        {
            if (OfferType == OfferType.Invoice && !InvoiceValue.HasValue)
            {
                return new GetOfferResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "Offer type invoice should have invoice value",
                    payload: null
                );
            }
            else if (OfferType == OfferType.Percentage && !PercentageValue.HasValue)
            {
                return new GetOfferResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "Offer type invoice should have invoice value",
                    payload: null
                );
            }
            return null;
        }

        private GetOfferResponse? ValidateRedeemByCode(
             bool RedeemByCode,
             string? RedeemCode
        )
        {
            if (RedeemByCode && string.IsNullOrEmpty(RedeemCode))
            {
                return new GetOfferResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "Redeem by code should have redeem code",
                    payload: null
                );
            }
            return null;
        }

        public async Task<GetBusinessOffersResponse> GetBusinessOffers(Guid businessId, EntityFilter filter)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetBusinessOffersResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var query = _offerRepository.GetOffersForFilter(false)
                    .Where(o => o.Business.Id == businessId);

                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(offer => offer.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(offer =>
                        offer.Title.ToLower().Contains(searchKeyword));
                }

                var totalItems = await query.CountAsync();
                var offers = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new GetBusinessOffersPayload
                {
                    Items = offers.Select(o =>
                    {
                        var item = _mapper.Map<BusinessOfferItem>(o);
                        item.OfferUrl = o.Logos.Select(logo => logo.LogoUrl).ToList();

                        item.Tags = o.GetOfferTagStatus()
                        .Select(tag => new OfferTag(tag.Type, tag.Tag, tag.TagColor))
                        .ToList();
                        return item;
                    }).ToList(),
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                return new GetBusinessOffersResponse
                (
                    StatusCodes.Status200OK,
                    "Business Offers",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating business offers");
                return new GetBusinessOffersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBusinessOffersResponse> GetBusinessActiveOffers(Entities.IndividualUser user, Guid businessId, LocationDTO? location)
        {
            try
            {
                var offers = (await _offerRepository.GetBusinessOffers(businessId, true)).Where(o => o.IsActive);
                var Items = offers.Select(r =>
                {
                    var response = _mapper.Map<BusinessOfferItem>(r);
                    response.Tags = r.GetOfferTagStatus()
                       .Select(tag => new OfferTag(tag.Type, tag.Tag, tag.TagColor))
                       .ToList();

                    response.OfferUrl = [];
                    foreach (var logo in r.Logos)
                    {
                        response.OfferUrl.Add(logo.LogoUrl);
                    }

                    if (location != null)
                    {
                        response.KmAway = (r.Business.GetNearestBranchWithDistance(location.Latitude, location.Longitude)).Distance;
                    }

                    return response;
                })
                .ToList();

                foreach (var item in Items)
                {
                    var redemption = await _userOfferRedeemRepository.GetByOfferAndUserId(user.Id, item.Id);
                    // Check if the offer is redeemed within the last 24 hours
                    item.IsRedeemByUser = redemption != null && DateTime.UtcNow < redemption.CreationTime.AddDays(1);
                    
                    // Add redemption time info if available
                    if (redemption != null)
                    {
                        item.LastRedeemTime = redemption.CreationTime;
                        item.CanRedeemAgainAt = redemption.CreationTime.AddDays(1);
                    }
                }

                var payload = new GetBusinessOffersPayload
                {
                    Items = Items,
                    TotalItems = offers.Count(),
                    HasNextPage = false
                };

                return new GetBusinessOffersResponse
                (
                    StatusCodes.Status200OK,
                    "Business Active Offers",
                    payload
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an business active offers");
                return new GetBusinessOffersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetOffersResponse> GetTopOffers(LocationDTO? location)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetOffersResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var offers = (await _offerRepository.GetTopOfferWithRelations())
                    .Where(offer => offer.IsActive)
                    .Take(10);

                var payload = new GetOffersPayload()
                {
                    Items = [],
                    TotalItems = offers.Count(),
                    HasNextPage = false
                };
                foreach (var o in offers)
                {
                    var item = _mapper.Map<OfferPayloadItem>(o);

                    item.Tags = o.GetOfferTagStatus()
                       .Select(tag => new OfferTag(tag.Type, tag.Tag, tag.TagColor))
                       .ToList();

                    item.OfferUrl = [];
                    foreach (var logo in o.Logos)
                    {
                        item.OfferUrl.Add(logo.LogoUrl);
                    }

                    if (location != null)
                    {
                        var nearestBranch = o.Business.GetNearestBranchWithDistance(location.Latitude, location.Longitude);
                        if (nearestBranch.Branch != null)
                        {
                            item.BranchPlaceId = nearestBranch.Branch.PlaceId;
                            item.BranchLatitude = nearestBranch.Branch.Latitude;
                            item.BranchLongitude = nearestBranch.Branch.Longitude;
                            item.branchId = nearestBranch.Branch.Id;
                            item.Business.Rating = nearestBranch.Branch.Rating;
                            item.Business.Review = nearestBranch.Branch.ReviewCount;
                        }
                        item.KmAway = nearestBranch.Distance;
                    }
                    item.SetBusinessNameLocalized(o.Business.GetNameByLanguageCode(languageCode));
                    item.Business.SetCategoryNameLocalized(o.Business.Category.GetNameByLanguageCode(languageCode));
                    payload.Items.Add(item);
                }

                return new GetOffersResponse
                (
                    StatusCodes.Status200OK,
                    "Business Active Offers",
                    payload
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting business top offers");
                return new GetOffersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetOffersResponse> GetOffersByFilter(EntityFilter filter, LocationDTO? location)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetOffersResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var query = _offerRepository.GetOffersForFilter(true);
                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(offer => offer.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(offer =>
                        offer.Title.ToLower().Contains(searchKeyword) ||
                        offer.Business.NameEn.ToLower().Contains(searchKeyword) ||
                        offer.Business.NameAr.ToLower().Contains(searchKeyword));
                }

                var totalItems = await query.CountAsync();
                var offers = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new GetOffersPayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                foreach (var offer in offers)
                {
                    var item = _mapper.Map<OfferPayloadItem>(offer);

                    item.Tags = offer.GetOfferTagStatus()
                       .Select(tag => new OfferTag(tag.Type, tag.Tag, tag.TagColor))
                       .ToList();

                    item.OfferUrl = [];
                    if (location != null)
                    {
                        item.KmAway = (offer.Business.GetNearestBranchWithDistance(location.Latitude, location.Longitude)).Distance;
                    }
                    foreach (var logo in offer.Logos)
                    {
                        item.OfferUrl.Add(logo.LogoUrl);
                    }
                    item.SetBusinessNameLocalized(offer.Business.GetNameByLanguageCode(languageCode));
                    payload.Items.Add(item);
                }

                return new GetOffersResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Offers Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching user");
                return new GetOffersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<RedeemOfferResponse> RedeemOffer(Entities.IndividualUser user, int offerId)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new RedeemOfferResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var offer = await _offerRepository.GetOfferWithRelations(offerId);
                if (offer == null)
                {
                    return new RedeemOfferResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Offer with Id {offerId} not found",
                        payload: null
                    );
                }

                if (offer.RedeemByCode == false)
                {
                    return new RedeemOfferResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"This offer cannot be redeem",
                        payload: null
                    );
                }

                var alreadyRedeemed = await _userOfferRedeemRepository.GetByOfferAndUserId(user.Id, offerId);
                if (alreadyRedeemed != null)
                {
                    // Check if the last redemption was more than 24 hours ago
                    if (DateTime.UtcNow < alreadyRedeemed.CreationTime.AddDays(1))
                    {
                        return new RedeemOfferResponse(
                            statusCode: StatusCodes.Status400BadRequest,
                            message: "You can redeem this offer again after 24 hours.",
                            payload: null
                        );
                    }
                    
                    // Create a new redemption record instead of updating the existing one
                    var newRedemptionRecord = new UserOfferRedeem
                    {
                        UserId = user.Id,
                        OfferId = offerId,
                        IsActive = true
                    };

                    await _userOfferRedeemRepository.InsertAsync(newRedemptionRecord, true);
                }
                else
                {
                    // First time redemption - create new record
                    var redemptionRecord = new UserOfferRedeem
                    {
                        UserId = user.Id,
                        OfferId = offerId,
                        IsActive = true
                    };

                    await _userOfferRedeemRepository.InsertAsync(redemptionRecord, true);
                }

                offer.RedeemCount += 1;
                await _offerRepository.UpdateAsync(offer, true);

                user.TotalPoints += offer.Points;
                await _individualUserRepository.UpdateAsync(user, true);

                var payload = _mapper.Map<RedeemOfferPayload>(offer);
                payload.Source = offer.Business.GetNameByLanguageCode(languageCode);

                return new RedeemOfferResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while getting offer with id {offerId}");
                return new RedeemOfferResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetOffersResponse> GetTopOfferByLocation(GetByLocationAndCategoryDTO model, Entities.IndividualUser user)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetOffersResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var userCityId = user.CityId;
                var userLatitude = model.Latitude;
                var userLongitude = model.Longitude;

                // Fetch active offers
                var offers = (await _offerRepository.GetTopOfferWithRelations())
                    .Where(o => o.IsActive)
                    .ToList();

                var filteredOffers = new List<OfferPayloadItem>();

                foreach (var offer in offers)
                {
                    // Filter by category (if specified)
                    bool matchesCategory =
                        string.IsNullOrEmpty(model.CategoryId) ||
                        int.Parse(model.CategoryId) == 0 ||
                        offer.Business.CategoryId == int.Parse(model.CategoryId);

                    foreach (var branch in offer.Business.Branches)
                    {
                        if (!branch.IsActive || (branch.Latitude == 0 && branch.Longitude == 0))
                            continue; // Skip inactive or invalid locations

                        bool isEligibleForOffer = false;

                        // If latitude and longitude are provided, check if the branch is within the radius
                        if (userLatitude.HasValue && userLongitude.HasValue)
                        {
                            if (string.IsNullOrEmpty(model.DistanceSlug))
                            {
                                model.DistanceSlug = "100";
                            }

                            if (!TryParseDistance(model.DistanceSlug, out double distanceValue))
                            {
                                distanceValue = 100;
                            }

                            isEligibleForOffer = LocationHelper.CalculateDistance(
                                (userLatitude.Value, userLongitude.Value),
                                (branch.Latitude, branch.Longitude)
                            ) <= distanceValue;
                        }
                        else
                        {
                            // If no latitude/longitude is provided, match by city
                            isEligibleForOffer = branch.CityId == userCityId;
                        }

                        if (matchesCategory && isEligibleForOffer)
                        {
                            var item = _mapper.Map<OfferPayloadItem>(offer);
                            item.OfferUrl = offer.Logos.Select(logo => logo.LogoUrl).ToList();

                            item.Tags = offer.GetOfferTagStatus()
                                .Select(tag => new OfferTag(tag.Type, tag.Tag, tag.TagColor))
                                .ToList();

                            item.SetBusinessNameLocalized(offer.Business.GetNameByLanguageCode(languageCode));
                            item.branchId = branch.Id;
                            item.SetBranchLocation(branch.Latitude, branch.Longitude, branch.PlaceId);
                            var (Rating, Review) = (branch.Rating, branch.ReviewCount);
                            item.Business.Rating = Rating;
                            item.Business.Review = Review;

                            filteredOffers.Add(item);
                        }
                    }
                }

                // Apply Discount Filtering
                if (!string.IsNullOrEmpty(model.DiscountSlug) &&
                    TryParseDiscount(model.DiscountSlug, out int discountThreshold))
                {
                    filteredOffers = filteredOffers
                        .Where(o => o.PercentageValue >= discountThreshold || o.InvoiceValue >= discountThreshold)
                        .ToList();
                }

                // Apply Sorting
                filteredOffers = model.SortBySlug switch
                {
                    "newest" => [.. filteredOffers.OrderByDescending(o => o.CreationTime)],
                    "oldest" => [.. filteredOffers.OrderBy(o => o.CreationTime)],
                    "a-z" => [.. filteredOffers.OrderBy(o => o.Title)],
                    "z-a" => [.. filteredOffers.OrderByDescending(o => o.Title)],
                    "top-rating" => [.. filteredOffers.OrderByDescending(o => o.Business.Rating)],
                    _ => filteredOffers
                };

                foreach (var item in filteredOffers)
                {
                    if (userLatitude.HasValue && userLongitude.HasValue)
                    {
                        var kmAway = LocationHelper.CalculateDistance(
                            (userLatitude.Value, userLongitude.Value),
                            (item.BranchLatitude, item.BranchLongitude)
                        );
                        item.KmAway = kmAway;
                    }
                }

                var items = filteredOffers.OrderBy(o => o.KmAway).Take(25).ToList();
                var totalItems = items.Count;
                var hasNextPage = filteredOffers.Count > 25;

                var payload = new GetOffersPayload
                {
                    Items = items,
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                return new GetOffersResponse(
                    StatusCodes.Status200OK,
                    "Business Active Offers",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting business top offers");
                return new GetOffersResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }


        //private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        //{
        //    const double R = 6371; // Radius of Earth in kilometers
        //    var latDistance = LocationHelper.DegreesToRadians(lat2 - lat1);
        //    var lonDistance = LocationHelper.DegreesToRadians(lon2 - lon1);

        //    var a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2) +
        //            Math.Cos(LocationHelper.DegreesToRadians(lat1)) * Math.Cos(LocationHelper.DegreesToRadians(lat2)) *
        //            Math.Sin(lonDistance / 2) * Math.Sin(lonDistance / 2);

        //    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        //    return R * c;
        //}

        public async Task<OfferStatusResponse> GetOfferStatus(Guid businessId)
        {
            try
            {
                var identityUser = await _userManager.FindByIdAsync(businessId.ToString());
                if (identityUser == null)
                {
                    return new OfferStatusResponse(
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: $"Business user with Id {businessId} not found",
                        payload: null
                    );
                }

                var existingBusiness = await _businessUserRepository.GetBusinessWithRelation(identityUser.Email);
                if (existingBusiness == null)
                {
                    return new OfferStatusResponse(
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: $"Business with Id {businessId} not found",
                        payload: null
                    );
                }

                var offers = await _offerRepository.GetBusinessOffers(existingBusiness.Id, true);
                var payload = new OfferStatusPayload
                {
                    TotalOffers = offers.Count,
                    ActiveOffers = offers.Where(o => o.IsActive).Count(),
                    InActiveOffers = offers.Where(o => !o.IsActive).Count(),
                };

                return new OfferStatusResponse
                (
                    StatusCodes.Status200OK,
                    "Business Offers",
                    payload
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an business offers");
                return new OfferStatusResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<UserRedeemOfferResponse> GetRedeemOffers(Entities.IndividualUser user)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new UserRedeemOfferResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var offers = await _userOfferRedeemRepository.GetUserRedeemedOffers(user.Id);
                var payload = new UserRedeemOfferPayload
                {
                    TotalPoints = user.TotalPoints,
                    Offers = []
                };

                foreach (var o in offers)
                {
                    var redeem = _mapper.Map<RedeemOfferPayload>(o);
                    redeem.Source = o.Business.GetNameByLanguageCode(languageCode);
                    payload.Offers.Add(redeem);
                }

                return new UserRedeemOfferResponse
                (
                    StatusCodes.Status200OK,
                    "Offers",
                    payload
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an offers");
                return new UserRedeemOfferResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetFilterByTypeResponse> GetItemByCategory(TypeFilterDTO model)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetFilterByTypeResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var query = _offerRepository.GetOffersForFilter(true)
                    .Where(o => o.IsActive);

                if (!string.IsNullOrEmpty(model.CategoryId) && int.TryParse(model.CategoryId, out int categoryId) && categoryId != 0)
                {
                    query = query.Where(offer => offer.Business.CategoryId == categoryId);
                }

                if (!string.IsNullOrEmpty(model.searchKeyword))
                {
                    var searchKeyword = model.searchKeyword.ToLower();
                    query = query.Where(offer =>
                        offer.Title.ToLower().Contains(searchKeyword) ||
                        (languageCode == Language.en ? offer.Business.NameEn.ToLower() : offer.Business.NameAr.ToLower())
                            .Contains(searchKeyword));
                }

                // Apply sorting before fetching data
                query = model.SortBySlug switch
                {
                    "newest" => query.OrderByDescending(o => o.StartDate),
                    "oldest" => query.OrderBy(o => o.StartDate),
                    "a-z" => query.OrderBy(o => o.Title),
                    "z-a" => query.OrderByDescending(o => o.Title),
                    _ => query
                };

                // Fetch data without pagination (to ensure filtering is applied correctly)
                var offers = await query
                    .Include(o => o.Business)
                        .ThenInclude(b => b.Branches)
                    .ToListAsync();

                if (model.SortBySlug == "top-rating")
                {
                    var items = await query.ToListAsync();
                    List<(Entities.Offer offers, double rating)> businessUsers = [];

                    foreach (var o in offers)
                    {
                        var (Branch, Distance) = o.Business.GetNearestBranchWithDistance(model.Latitude, model.Longitude);
                        var (Rating, Review) = (0D, 0);
                        if (Branch != null)
                        {
                            (Rating, Review) = (Branch.Rating, Branch.ReviewCount);
                        }
                        businessUsers.Add((o, Rating));
                    }

                    offers = businessUsers.OrderByDescending(x => x.rating).Select(x => x.offers).ToList();
                }

                // Apply distance filtering in-memory
                if (!string.IsNullOrEmpty(model.DistanceSlug) &&
                    TryParseDistance(model.DistanceSlug, out double maxDistance) &&
                    model.Latitude.HasValue && model.Longitude.HasValue)
                {
                    offers = offers.Where(offer =>
                        offer.Business.Branches.Any(branch =>
                            branch.IsActive &&
                            branch.Latitude != 0 && branch.Longitude != 0 &&
                            LocationHelper.CalculateDistance((model.Latitude.Value, model.Longitude.Value), (branch.Latitude, branch.Longitude)) <= maxDistance
                        )
                    ).ToList();
                }

                // Apply discount filtering in-memory
                if (!string.IsNullOrEmpty(model.DiscountSlug) && TryParseDiscount(model.DiscountSlug, out int discountThreshold))
                {
                    offers = offers.Where(
                        o => o.PercentageValue >= discountThreshold ||
                             o.InvoiceValue >= discountThreshold
                    ).ToList();
                }

                // Get total count after filtering
                var totalItems = offers.Count;

                // 🔥 Apply pagination at the end!
                var paginatedOffers = offers
                    .Skip((model.pageNumber - 1) * model.pageSize)
                    .Take(model.pageSize)
                    .ToList();

                // Check if there's a next page
                bool hasNextPage = (model.pageNumber * model.pageSize) < totalItems;

                var payload = new CategoryItemPayload
                {
                    Type = "Offer",
                    Items = [],
                    TotalItems = paginatedOffers.Count,
                    HasNextPage = hasNextPage
                };

                foreach (var offer in paginatedOffers)
                {
                    var (Branch, Distance) = offer.Business.GetNearestBranchWithDistance(model.Latitude, model.Longitude);
                    var (Rating, Review) = (0D, 0);
                    if (Branch != null)
                    {
                        (Rating, Review) = (Branch.Rating, Branch.ReviewCount);
                    }

                    var business = new BusinessTypeItem
                    {
                        id = offer.Business.Id,
                        Name = offer.Business.GetNameByLanguageCode(languageCode),
                        Category = offer.Business.Category.GetNameByLanguageCode(languageCode),
                        Logo = offer.Business.LogoPath,
                        KmAway = Distance,
                        Rating = Rating,
                        Review = Review
                    };

                    var offerItem = new OfferTypeItem
                    {
                        Id = offer.Id,
                        Title = offer.Title,
                        Description = offer.Description,
                        OfferUrl = offer.Logos.Select(l => l.LogoUrl).ToList(),
                        Tags = [],
                        OfferType = offer.OfferType,
                        PercentageValue = offer.PercentageValue,
                        InvoiceValue = offer.InvoiceValue,
                        business = business,
                    };

                    offerItem.Tags = offer.GetOfferTagStatus()
                    .Select(tag => new OfferTag(tag.Type, tag.Tag, tag.TagColor))
                    .ToList();

                    payload.Items.Add(offerItem);
                }

                return new GetFilterByTypeResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Offers retrieved successfully",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching offer item");
                return new GetFilterByTypeResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        private bool TryParseDistance(string slug, out double distance)
        {
            return double.TryParse(slug.Replace("km", ""), out distance);
        }

        private bool TryParseDiscount(string slug, out int discount)
        {
            return int.TryParse(slug.Replace("%", ""), out discount);
        }

        public async Task<RedeemOfferQrResponse> RedeemOfferByQrCode(Entities.IndividualUser user, RedeemOfferByQrDTO model)
        {
            try
            {
                var branch = await _branchRepository.GetByIdAsync(model.BranchId);
                if (branch == null)
                {
                    return new RedeemOfferQrResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Invalid branch",
                        payload: null
                    );
                }

                var userToken = await _userOfferTokenRepository.GetByUserId(user.Id);
                if (userToken == null)
                {
                    userToken = new UserOfferToken
                    {
                        UserId = user.Id,
                        BranchId = model.BranchId,
                        ExpiryTime = DateTime.UtcNow.AddMinutes(10),
                        Code = GenerateToken()
                    };
                    await _userOfferTokenRepository.InsertAsync(userToken, true);
                }
                else
                {
                    userToken.Code = GenerateToken();
                    userToken.BranchId = model.BranchId;
                    userToken.ExpiryTime = DateTime.UtcNow.AddMinutes(10);
                    await _userOfferTokenRepository.UpdateAsync(userToken, true);
                }

                var payload = new RedeemOfferQrPayload
                {
                    Code = userToken.Code,
                    ExpiryTime = userToken.ExpiryTime,
                };

                return new RedeemOfferQrResponse
                (
                    StatusCodes.Status200OK,
                    "Redeem Code",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while redeeming offers");
                return new RedeemOfferQrResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        private static readonly string chars = "0123456789";
        private static readonly Random random = new();
        public static string GenerateToken(int length = 5)
        {
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<GetSearchFilters> GetSearchFilter()
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetSearchFilters(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var categories = (await _categoryRepository.GetAsync())
                    .Where(x => x.IsActive = true);

                var paylod = new SearchFilterPayload()
                {
                    Filter =
                    [
                        new()
                        {
                            Name = "SortBy",
                            Lookup = "sort",
                            Items =
                            [
                                new() { Text = "Newest to Old", Slug = "newest" },
                                new() { Text = "Old to Newest", Slug = "oldest" },
                                new() { Text = "A to Z", Slug = "a-z" },
                                new() { Text = "Z to A", Slug = "z-a" },
                                new() { Text = "Top Rating", Slug = "top-rating" }
                            ]
                        },
                        new()
                        {
                            Name = "Distance to me",
                            Lookup = "distance",
                            Items =
                            [
                                new() { Text = "20 km", Slug = "20km" },
                                new() { Text = "15 km", Slug = "15km" },
                                new() { Text = "10 km", Slug = "10km" },
                                new() { Text = "5 km", Slug = "5km" },
                                new() { Text = "4 km", Slug = "4km" },
                                new() { Text = "3 km", Slug = "3km" },
                                new() { Text = "2 km", Slug = "2km" },
                                new() { Text = "1 km", Slug = "1km" }
                            ]
                        },
                        new()
                        {
                            Name = "Discount",
                            Lookup = "discount",
                            Items =
                            [
                                new() { Text = "75%", Slug = "75" },
                                new() { Text = "70%", Slug = "70" },
                                new() { Text = "50%", Slug = "50" },
                                new() { Text = "40%", Slug = "40" },
                                new() { Text = "30%", Slug = "30" },
                                new() { Text = "25%", Slug = "25" },
                                new() { Text = "20%", Slug = "20" },
                                new() { Text = "15%", Slug = "15" },
                                new() { Text = "10%", Slug = "10" },
                                new() { Text = "5%", Slug = "5" }
                            ]
                        }
                    ]
                };

                var categoryItem = new FilterCategory
                {
                    Name = "Categories",
                    Lookup = "category",
                    Items = []
                };

                foreach (var c in categories)
                {
                    categoryItem.Items.Add(new FilterItem
                    {
                        Text = c.GetNameByLanguageCode(languageCode),
                        Slug = c.Id.ToString(),
                        ImageUrl = c.ImagePath
                    });
                }

                paylod.Filter.Add(categoryItem);

                return new GetSearchFilters
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Get filter success",
                    payload: paylod
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching filters");
                return new GetSearchFilters
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBusinessForOffersResponse> GetBusinessForOffers()
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetBusinessForOffersResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var response = (await _businessUserRepository.GetAsync())
                    .Where(business => business.IsActive);

                var payload = new GetBusinessForOffersPayload()
                {
                    Items = []
                };

                foreach (var r in response)
                {
                    var item = new BusinessForOffersItem()
                    {
                        Id = r.Id,
                        Name = r.GetNameByLanguageCode(languageCode),
                    };
                    payload.Items.Add(item);
                }

                return new GetBusinessForOffersResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Business Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching business user");
                return new GetBusinessForOffersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }
    }
}
