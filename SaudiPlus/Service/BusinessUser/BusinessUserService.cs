using AutoMapper;
using SaudiPlus.Enums;
using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Entities;
using SaudiPlus.Extensions;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Bucket;
using SaudiPlus.Request.Offer;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Helper.Location;
using SaudiPlus.Response.Policy;
using SaudiPlus.Request.Category;
using SaudiPlus.Response.Category;
using SaudiPlus.Entities.Identity;
using SaudiPlus.Interfaces.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using SaudiPlus.Request.BusinessUser;
using SaudiPlus.Response.BusinessUser;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Service.BusinessUser
{
    public class BusinessUserService : BaseService<BusinessUserService>, IBusinessUserService
    {
        private readonly BucketSetting _bucket;
        private readonly IMapService _mapService;
        private readonly IFileService _fileService;
        private readonly SaudiPlusDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IOfferRepository _offerRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IFavouriteRepository _favoriteRepository;
        private readonly IBusinessRepository _businessUserRepository;
        private readonly IIndividualUserRepository _individualUserRepository;
        private readonly IBusinessPolicyRepository _businessPolicyRepository;

        public BusinessUserService(
            IMapper mapper,
            IMapService mapService,
            IFileService fileService,
            SaudiPlusDbContext dbContext,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptions<BucketSetting> bucket,
            ILanguageService languageService,
            IOfferRepository offerRepository,
            IPolicyRepository policyRepository,
            IBranchRepository branchRepository,
            ILogger<BusinessUserService> logger,
            IInvoiceRepository invoiceRepository,
            IFavouriteRepository favoriteRepository,
            IBusinessRepository businessUserRepository,
            IIndividualUserRepository individualUserRepository,
            IBusinessPolicyRepository businessPolicyRepository
        ) : base(mapper, logger, languageService)
        {
            _bucket = bucket.Value;
            _dbContext = dbContext;
            _mapService = mapService;
            _userManager = userManager;
            _roleManager = roleManager;
            _fileService = fileService;
            _offerRepository = offerRepository;
            _policyRepository = policyRepository;
            _branchRepository = branchRepository;
            _invoiceRepository = invoiceRepository;
            _favoriteRepository = favoriteRepository;
            _businessUserRepository = businessUserRepository;
            _individualUserRepository = individualUserRepository;
            _businessPolicyRepository = businessPolicyRepository;
        }

        public async Task<SuccessResponse> CreateBusinessUser(CreateBusinessDTO model)
        {
            var error = await _userManager.ValidateEmailUniqueness(model.Email);
            if (error != null)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: error.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            try
            {
                var businessUser = _mapper.Map<Entities.BusinessUser>(model);
                businessUser.NameAr = model.Name;
                businessUser.NameEn = model.Name;

                await _businessUserRepository.InsertAsync(businessUser);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Business Created",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> AdminCreateBusinessUser(AdminCreateBusinessDTO model)
        {
            var uploadResponse = await _fileService.UploadFileAsync(model.Logo, _bucket.Business + "/" + model.NameEn);
            if (uploadResponse.error != null)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: uploadResponse.error.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            var error = await _userManager.ValidateEmailUniqueness(model.Email);
            if (error != null)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: error.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var newUser = _mapper.Map<User>(model);
                    newUser.UserName = model.Email.Split('@')[0] + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);
                    var createResult = await _userManager.CreateAsync(newUser, model.Password);

                    if (!createResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return new SuccessResponse
                        (
                            statusCode: StatusCodes.Status500InternalServerError,
                            message: string.Join(", ", createResult.Errors.Select(e => e.Description)),
                            payload: new SuccessResponsePayload { success = false }
                        );
                    }

                    await _roleManager.EnsureRoleExistsAsync(UserRole.Business.ToString(), newUser.Id);
                    await _userManager.AddUserToRole(newUser, UserRole.Business.ToString());

                    var businessUser = await _businessUserRepository.GetBusinessWithRelation(model.Email);
                    if (businessUser != null)
                    {
                        _mapper.Map(model, businessUser);
                    }
                    else
                    {
                        businessUser = _mapper.Map<Entities.BusinessUser>(model);
                        businessUser.CompanyName = model.NameEn;
                    }

                    businessUser.LogoPath = uploadResponse.URL;
                    businessUser.UserId = newUser.Id;
                    businessUser.IsActive = true;

                    await _businessUserRepository.InsertAsync(businessUser, true);

                    await transaction.CommitAsync();
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status200OK,
                        message: "Business Created",
                        payload: new SuccessResponsePayload { success = true }
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while creating an business user");
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "An unexpected error occurred. Please try again later.",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }
            });
        }

        public async Task<SuccessResponse> EditBusiness(Guid businessId, EditBusinessDTO model)
        {

            var existingBusiness = await _businessUserRepository.GetBusinessWithRelation(businessId);
            if (existingBusiness == null)
            {
                return new SuccessResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: $"Business with Id {businessId} not found",
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            var identityUser = await _userManager.FindByIdAsync(existingBusiness.UserId.ToString());
            if (identityUser == null)
            {
                return new SuccessResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: $"Business user with Id {businessId} not found",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    model.categoryId = model.categoryId.HasValue ? model.categoryId.Value : existingBusiness.CategoryId;
                    model.IsActive = model.IsActive.HasValue ? model.IsActive.Value : existingBusiness.IsActive;

                    _mapper.Map(model, identityUser);
                    _mapper.Map(model, existingBusiness);

                    await _userManager.UpdateAsync(identityUser);

                    (string URL, Exception? error) uploadResponse = ("", null);
                    if (model.Logo != null)
                    {
                        if (!string.IsNullOrEmpty(existingBusiness.LogoPath))
                        {
                            await _fileService.DeleteFileAsync(existingBusiness.LogoPath);
                        }

                        uploadResponse = await _fileService.UploadFileAsync(model.Logo, _bucket.Business + "/" + existingBusiness.NameEn);

                        if (uploadResponse.error != null)
                        {
                            await transaction.RollbackAsync();
                            return new SuccessResponse
                            (
                                statusCode: StatusCodes.Status400BadRequest,
                                message: uploadResponse.error.Message,
                                payload: new SuccessResponsePayload { success = false }
                            );
                        }
                    }

                    if (!string.IsNullOrEmpty(uploadResponse.URL))
                    {
                        existingBusiness.LogoPath = uploadResponse.URL;
                    }

                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                        var result = await _userManager.ResetPasswordAsync(identityUser, token, model.Password);

                        if (!result.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            return new SuccessResponse(
                                StatusCodes.Status400BadRequest,
                                $"Password reset failed: {errors}",
                                new SuccessResponsePayload { success = false }
                            );
                        }
                    }

                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        var token = await _userManager.GenerateChangeEmailTokenAsync(identityUser, model.Email);
                        var result = await _userManager.ChangeEmailAsync(identityUser, model.Email, token);

                        if (!result.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            return new SuccessResponse(
                                StatusCodes.Status400BadRequest,
                                $"Email change failed: {errors}",
                                new SuccessResponsePayload { success = false }
                            );
                        }
                    }

                    await _businessUserRepository.UpdateAsync(existingBusiness, true);
                    var payload = _mapper.Map<BusinessInfoDetailed>(existingBusiness);

                    await transaction.CommitAsync();
                    return new SuccessResponse(
                        statusCode: StatusCodes.Status200OK,
                        message: "Business Updated",
                        payload: new SuccessResponsePayload { success = true }
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while editing business user");
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "An unexpected error occurred. Please try again later.",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }
            });
        }

        public async Task<SuccessResponse> ToggleBusinessStatus(ToggleStatusDTO<Guid> model)
        {
            try
            {
                var businessUser = await _businessUserRepository.GetBusinessWithRelation(model.Id);
                if (businessUser == null)
                {
                    return new SuccessResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Entity with Id {model.Id} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                //await _individualUserRepository.BeginTransactionAsync();
                businessUser.IsActive = model.IsActive;
                await _businessUserRepository.UpdateAsync(businessUser, true);
                //await _individualUserRepository.CommitTransactionAsync();

                return new SuccessResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "Business User Updated",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                //await _individualUserRepository.RollbackTransactionAsync();
                _logger.LogError(ex, "An error occurred editing user");

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetBusinessesResponse> GetBusiness()
        {
            try
            {
                var response = await _businessUserRepository.GetBusinessWithCategory();
                var payload = new BusinessPayload()
                {
                    Items = [],
                    TotalItems = response.Count(),
                    HasNextPage = false
                };

                var howToUse = await _policyRepository.GetPolicyByType(PolicyType.HowToUse);

                foreach (var r in response)
                {
                    var item = _mapper.Map<BusinessInfoDetailed>(r);
                    item.HowToUseAr = howToUse == null ? "" : howToUse.ContentAr;
                    item.HowToUseEn = howToUse == null ? "" : howToUse.ContentEn;
                    payload.Items.Add(item);
                }

                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Business Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching business user");
                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBusinessResponse> GetBusinessByIdDetailed(Guid businessId)
        {
            try
            {
                var response = await _businessUserRepository.GetBusinessWithRelation(businessId);
                if (response == null)
                {
                    return new GetBusinessResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: $"Entity with Id {businessId} not found",
                        payload: null
                    );
                }

                var howToUse = await _policyRepository.GetPolicyByType(PolicyType.HowToUse);
                var payload = _mapper.Map<BusinessInfoDetailed>(response);
                payload.HowToUseAr = howToUse == null ? "" : howToUse.ContentAr;
                payload.HowToUseEn = howToUse == null ? "" : howToUse.ContentEn;

                return new GetBusinessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Business Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching business user with id : " + businessId);
                return new GetBusinessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBusinessResponse> GetBusinessByIdLocalized(Guid businessId)
        {
            try
            {
                var response = await _businessUserRepository.GetBusinessWithRelation(businessId);
                if (response == null)
                {
                    return new GetBusinessResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: $"Entity with Id {businessId} not found",
                        payload: null
                    );
                }

                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBusinessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var howToUse = await _policyRepository.GetPolicyByType(PolicyType.HowToUse);
                var payload = _mapper.Map<BusinessInfoLocalized>(response);
                payload.Name = response.GetNameByLanguageCode(languageCode);
                payload.SetCategoryNameLocalized(response.Category.GetNameByLanguageCode(languageCode));
                payload.HowToUse = howToUse!.GetContentByLanguageCode(languageCode);

                return new GetBusinessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Business Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching business user with id : " + businessId);
                return new GetBusinessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBusinessesResponse> GetActiveBusiness()
        {
            try
            {
                var response = (await _businessUserRepository.GetBusinessWithCategory())
                    .Where(business => business.IsActive);

                var payload = new BusinessPayload()
                {
                    Items = [],
                    TotalItems = response.Count(),
                    HasNextPage = false
                };

                var howToUse = await _policyRepository.GetPolicyByType(PolicyType.HowToUse);

                foreach (var r in response)
                {
                    var item = _mapper.Map<BusinessInfoDetailed>(r);
                    item.HowToUseAr = howToUse == null ? "" : howToUse.ContentAr;
                    item.HowToUseEn = howToUse == null ? "" : howToUse.ContentEn;
                    payload.Items.Add(item);
                }

                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Business Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching business user");
                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBusinessesResponse> GetActiveBusinessLocalized()
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBusinessesResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var response = (await _businessUserRepository.GetBusinessWithCategory())
                    .Where(business => business.IsActive);

                var payload = new BusinessPayload()
                {
                    Items = [],
                    TotalItems = response.Count(),
                    HasNextPage = false
                };

                var howToUse = await _policyRepository.GetPolicyByType(PolicyType.HowToUse);
                foreach (var r in response)
                {
                    var item = _mapper.Map<BusinessInfoLocalized>(r);
                    item.Name = r.GetNameByLanguageCode(languageCode);
                    item.SetCategoryNameLocalized(r.Category.GetNameByLanguageCode(languageCode));
                    item.HowToUse = howToUse!.GetContentByLanguageCode(languageCode);
                    payload.Items.Add(item);
                }

                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Business Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching business user");
                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBusinessesResponse> GetTopBusinessLocalized()
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBusinessesResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var response = (await _businessUserRepository.GetBusinessWithCategory())
                    .Where(business => business.IsActive)
                    .Take(10);

                var payload = new BusinessPayload()
                {
                    Items = [],
                    TotalItems = response.Count(),
                    HasNextPage = false
                };
                foreach (var r in response)
                {
                    var item = _mapper.Map<BusinessInfoLocalized>(r);
                    item.Name = r.GetNameByLanguageCode(languageCode);
                    item.SetCategoryNameLocalized(r.Category.GetNameByLanguageCode(languageCode));
                    payload.Items.Add(item);
                }

                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Business Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching business user");
                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBusinessesResponse> GetBusinessByFilter(EntityFilter paginationFilter)
        {
            try
            {
                var query = _businessUserRepository.GetBusinessForFilter();

                if (paginationFilter.isActive.HasValue)
                {
                    query = query
                        .Where(business => business.IsActive == paginationFilter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(paginationFilter.searchKeyword))
                {
                    var searchKeyword = paginationFilter.searchKeyword.ToLower();
                    query = query.Where(business =>
                        business.NameEn.ToLower().Contains(searchKeyword) ||
                        business.Address.ToLower().Contains(searchKeyword) ||
                        business.Category.NameEn.ToLower().Contains(searchKeyword));
                }

                var totalItems = await query.CountAsync();
                var businesses = await query
                    .Skip((paginationFilter.pageNumber - 1) * paginationFilter.pageSize)
                    .Take(paginationFilter.pageSize)
                    .ToListAsync();
                bool hasNextPage = (paginationFilter.pageNumber * paginationFilter.pageSize) < totalItems;

                var payload = new BusinessPayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };
                foreach (var business in businesses)
                {
                    var item = _mapper.Map<BusinessInfoDetailed>(business);
                    payload.Items.Add(item);
                }

                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Business Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching business user");
                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBusinessesResponse> GetBusinessByFilterLocalized(EntityFilter paginationFilter)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBusinessesResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var query = _businessUserRepository.GetBusinessForFilter();

                if (paginationFilter.isActive.HasValue)
                {
                    query = query
                        .Where(business => business.IsActive == paginationFilter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(paginationFilter.searchKeyword))
                {
                    var searchKeyword = paginationFilter.searchKeyword.ToLower();
                    query = query.Where(business =>
                        business.NameEn.ToLower().Contains(searchKeyword) ||
                        business.Address.ToLower().Contains(searchKeyword) ||
                        business.Category.NameEn.ToLower().Contains(searchKeyword));
                }

                var totalItems = await query.CountAsync();
                var businesses = await query
                    .Skip((paginationFilter.pageNumber - 1) * paginationFilter.pageSize)
                    .Take(paginationFilter.pageSize)
                    .ToListAsync();
                bool hasNextPage = (paginationFilter.pageNumber * paginationFilter.pageSize) < totalItems;

                var payload = new BusinessPayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };
                foreach (var business in businesses)
                {
                    var item = _mapper.Map<BusinessInfoLocalized>(business);
                    item.Name = business.GetNameByLanguageCode(languageCode);
                    item.SetCategoryNameLocalized(business.Category.GetNameByLanguageCode(languageCode));
                    payload.Items.Add(item);
                }

                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Business Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching business user");
                return new GetBusinessesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> DeleteBusiness(Guid businessId)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var business = await _businessUserRepository.GetByIdAsync(businessId);
                    if (business == null)
                    {
                        return new SuccessResponse(
                            statusCode: StatusCodes.Status400BadRequest,
                            message: $"Entity with Id {businessId} not found",
                            payload: new SuccessResponsePayload { success = false }
                        );
                    }

                    var user = await _userManager.FindByIdAsync(business.UserId.ToString());
                    if (user == null)
                    {
                        return new SuccessResponse(
                            statusCode: StatusCodes.Status500InternalServerError,
                            message: $"User with Id {businessId} not found",
                            payload: new SuccessResponsePayload { success = false }
                        );
                    }

                    if (!string.IsNullOrEmpty(business.LogoPath))
                    {
                        await _fileService.DeleteFileAsync(business.LogoPath);
                    }
                    await _userManager.DeleteAsync(user);
                    await _businessUserRepository.DeleteAsync(businessId, true);

                    await transaction.CommitAsync();

                    return new SuccessResponse(
                        statusCode: StatusCodes.Status200OK,
                        message: "Business Deleted",
                        payload: new SuccessResponsePayload { success = true }
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while deleting an business user");
                    await transaction.RollbackAsync();
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "An unexpected error occurred. Please try again later.",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }
            });
        }

        public async Task<SuccessResponse> SetBusinessContact(Entities.BusinessUser business, BusinessContactDTO model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(business.UserId.ToString());
                if (user == null)
                {
                    return new SuccessResponse(
                        StatusCodes.Status404NotFound,
                        "Enable to find the user",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                if (!string.IsNullOrEmpty(model.Email))
                {
                    var emailToken = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
                    var result = await _userManager.ChangeEmailAsync(user, model.Email, emailToken);

                    if (!result.Succeeded)
                    {
                        return new SuccessResponse
                        (
                            statusCode: StatusCodes.Status500InternalServerError,
                            message: "An unexpected error occurred while updating email",
                            payload: new SuccessResponsePayload { success = false }
                        );
                    }
                }

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    var phoneToken = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
                    var result = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, phoneToken);

                    if (!result.Succeeded)
                    {
                        return new SuccessResponse
                        (
                            statusCode: StatusCodes.Status500InternalServerError,
                            message: "An unexpected error occurred while updating phone number",
                            payload: new SuccessResponsePayload { success = false }
                        );
                    }
                }

                _mapper.Map(model, business);
                await _businessUserRepository.UpdateAsync(business, true);

                return new SuccessResponse(
                    StatusCodes.Status200OK,
                    "Set Business Contact Success",
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

        public async Task<SuccessResponse> SetBusinessSocialMedia(Entities.BusinessUser business, SetSocialMediaDto model)
        {
            try
            {
                if (model.Type == null)
                {
                    return new SuccessResponse(
                        StatusCodes.Status404NotFound,
                        "Type cannot be null",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                if (model.Type.ToLower() == "Facebook".ToLower())
                {
                    business.FacebookUrl = model.Url;
                    business.FacebookPage = model.Page;
                }
                else if (model.Type.ToLower() == "Instagram".ToLower())
                {
                    business.InstagramUrl = model.Url;
                    business.InstagramPage = model.Page;
                }
                else if (model.Type.ToLower() == "Snapchat".ToLower())
                {
                    business.SnapchatUrl = model.Url;
                    business.SnapchatPage = model.Page;
                }
                else if (model.Type.ToLower() == "Twitter".ToLower())
                {
                    business.TwitterUrl = model.Url;
                    business.TwitterPage = model.Page;
                }
                else
                {
                    return new SuccessResponse(
                       StatusCodes.Status404NotFound,
                       "Invalid Type",
                       payload: new SuccessResponsePayload { success = false }
                   );
                }

                await _businessUserRepository.UpdateAsync(business, true);

                return new SuccessResponse(
                    StatusCodes.Status200OK,
                    $"{model.Type} Update Success",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting an business user");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetBusinessContactResponse> GetBusinessContact(Guid businessId)
        {
            try
            {
                var business = await _businessUserRepository.GetBusinessWithRelation(businessId);
                if (business == null)
                {
                    return new GetBusinessContactResponse(
                        StatusCodes.Status404NotFound,
                        "This user is not associated with any business",
                        payload: null
                    );
                }

                var payload = _mapper.Map<GetBusinessContactPayload>(business);

                return new GetBusinessContactResponse(
                    StatusCodes.Status200OK,
                    "Business Contact",
                    payload: payload
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting an business user");
                return new GetBusinessContactResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<BusinessStatsResponse> GetBusinessStats(Guid businessId)
        {
            try
            {
                var business = await _businessUserRepository.GetBusinessWithRelation(businessId);
                if (business == null)
                {
                    return new BusinessStatsResponse(
                        StatusCodes.Status404NotFound,
                        "This user is not associated with any business",
                        payload: null
                    );
                }

                var offers = await _offerRepository.GetBusinessOffers(business.Id, true);
                var redeemCount = offers.Sum(o => o.RedeemCount);
                var branches = await _branchRepository.GetBusinessBranches(business.Id);
                var invoices = await _invoiceRepository.GetInvoiceByBusinessId(business.Id);
                var totalSales = invoices.Sum(i => i.TotalAmount);

                var payload = new BusinessStatsPayload
                {
                    TotalRedeemCount = redeemCount,
                    TotalSalesAmount = totalSales,
                    TotalBranches = branches.Count,
                    TotalOffers = offers.Count(),
                };

                return new BusinessStatsResponse(
                    StatusCodes.Status200OK,
                    "Business Stats",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting business stats");
                return new BusinessStatsResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBusinessSalesResponse> GetMonthlySales(Guid businessId, int? year)
        {
            try
            {
                var allInvoices = (await _invoiceRepository.GetAsync()).Where(Invoice => Invoice.IsVerified);

                // Determine whether to use the provided year or calculate the last 12 months
                DateTime now = DateTime.UtcNow;
                var targetMonths = year > 0
                    ? Enumerable.Range(1, 12).Select(month => new { Year = year.Value, Month = month })
                    : Enumerable.Range(0, 12).Select(offset => new { Year = now.AddMonths(-offset).Year, Month = now.AddMonths(-offset).Month });

                var filteredInvoices = allInvoices
                    .Where(invoice => invoice.BusinessId == businessId && targetMonths.Any(target => target.Year == invoice.IssueDate.Year && target.Month == invoice.IssueDate.Month))
                    .ToList();

                // Aggregate stats for the selected months
                var stats = targetMonths
                    .GroupJoin(
                        filteredInvoices,
                        target => new { target.Year, target.Month },
                        invoice => new { invoice.IssueDate.Year, invoice.IssueDate.Month },
                        (target, invoices) => new BusinessSalesState
                        {
                            Year = target.Year,
                            Month = target.Month,
                            TotalSalesAmount = invoices.Sum(invoice => invoice.TotalAmount)
                        })
                    .OrderBy(stat => stat.Year).ThenBy(stat => stat.Month)
                    .ToList();

                return new GetBusinessSalesResponse(
                    StatusCodes.Status200OK,
                    "Business Sales Stats",
                    payload: new GetBusinessSalesPayload { Stats = stats }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting business sales stats");
                return new GetBusinessSalesResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetOfferRedeemStateResponse> GetMonthlyOfferRedeemCount(Guid businessId, int? year)
        {
            try
            {
                var allInvoices = (await _invoiceRepository.GetAsync()).Where(Invoice => Invoice.IsVerified);

                // Determine whether to use the provided year or calculate the last 12 months
                DateTime now = DateTime.UtcNow;
                var targetMonths = year > 0
                    ? Enumerable.Range(1, 12).Select(month => new { Year = year.Value, Month = month })
                    : Enumerable.Range(0, 12).Select(offset => new { Year = now.AddMonths(-offset).Year, Month = now.AddMonths(-offset).Month });

                var filteredInvoices = allInvoices
                    .Where(invoice => invoice.BusinessId == businessId && targetMonths.Any(target => target.Year == invoice.IssueDate.Year && target.Month == invoice.IssueDate.Month))
                    .ToList();

                // Aggregate stats for the selected months
                var stats = targetMonths
                    .GroupJoin(
                        filteredInvoices,
                        target => new { target.Year, target.Month },
                        invoice => new { invoice.IssueDate.Year, invoice.IssueDate.Month },
                        (target, invoices) => new OfferRedeemStat
                        {
                            Year = target.Year,
                            Month = target.Month,
                            TotalRedeemCount = invoices.Count()
                        })
                    .OrderBy(stat => stat.Year).ThenBy(stat => stat.Month)
                    .ToList();

                return new GetOfferRedeemStateResponse(
                    StatusCodes.Status200OK,
                    "Offer Redeem Stats",
                    payload: new OfferRedeemStatePayload { Stats = stats }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting offer redeem stats");
                return new GetOfferRedeemStateResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBusinessByLocationResponse> GetBusinessByLocation(Entities.IndividualUser user, GetByLocationAndCategoryDTO model)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetBusinessByLocationResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var userCityId = user.CityId;
                var userLatitude = model.Latitude;
                var userLongitude = model.Longitude;

                var business = (await _businessUserRepository.GetBusinessForLocation())
                    .Where(b => b.IsActive)
                    .ToList();

                var filteredBusiness = business.Where(o =>
                {
                    // Filter by Category
                    var matchesCategory =
                       string.IsNullOrEmpty(model.CategoryId) ||
                        int.Parse(model.CategoryId) == 0 ||
                       o.CategoryId == int.Parse(model.CategoryId);

                    // If Latitude and Longitude are provided, filter by distance
                    if (userLatitude.HasValue && userLongitude.HasValue)
                    {
                        var branchLocations = o.Branches
                            .Where(branch =>
                                branch.IsActive &&
                                !(branch.Latitude == 0 && branch.Longitude == 0)) // Exclude invalid coordinates
                            .ToList();

                        bool isWithinRadius = false;
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

                            isWithinRadius = branchLocations.Any(branch =>
                            {
                                return CalculateDistance(
                                    userLatitude.Value,
                                    userLongitude.Value,
                                    branch.Latitude,
                                    branch.Longitude
                                ) <= distanceValue;
                            });
                        }

                        return matchesCategory && isWithinRadius;
                    }
                    else
                    {
                        // If Latitude and Longitude are NOT provided, filter by City
                        bool matchesCity = userCityId.HasValue && o.Branches.Any(branch =>
                            branch.IsActive &&
                            branch.CityId == userCityId);

                        return matchesCategory && matchesCity;
                    }
                }).ToList();

                // Apply Discount Filtering
                if (!string.IsNullOrEmpty(model.DiscountSlug) &&
                    TryParseDiscount(model.DiscountSlug, out int discountThreshold))
                {
                    filteredBusiness = filteredBusiness
                        .Where(b => b.Offers.Any(o => o.PercentageValue >= discountThreshold || o.InvoiceValue >= discountThreshold))
                        .ToList();
                }

                List<(Entities.BusinessUser business, double rating)> businessUsers = new();
                foreach (var b in filteredBusiness)
                {
                    var (Branch, Distance) = b.GetNearestBranchWithDistance(userLatitude, userLongitude);
                    var (Rating, Review) = (0D, 0);
                    if (Branch != null)
                    {
                        (Rating, Review) = (Branch.Rating, Branch.ReviewCount); 
                    }
                    businessUsers.Add((b, Rating));
                }

                // Apply Sorting
                businessUsers = model.SortBySlug switch
                {
                    "newest" => [.. businessUsers.OrderByDescending(b => b.business.CreationTime)],
                    "oldest" => [.. businessUsers.OrderBy(b => b.business.CreationTime)],
                    "a-z" => [.. businessUsers.OrderBy(b => b.business.NameEn)],
                    "z-a" => [.. businessUsers.OrderByDescending(b => b.business.NameEn)],
                    "top-rating" => [.. businessUsers.OrderByDescending(b => b.rating)],
                    _ => businessUsers
                };

                List<BusinessLocationItem> Items = [];

                foreach (var b in filteredBusiness)
                {
                    // Collect branches based on user location or city
                    var matchingBranches = userLatitude.HasValue && userLongitude.HasValue
                        ? b.Branches
                            .Where(branch =>
                                branch.IsActive &&
                                !(branch.Latitude == 0 && branch.Longitude == 0) && // Exclude invalid coordinates
                                CalculateDistance(userLatitude.Value, userLongitude.Value, branch.Latitude, branch.Longitude) <= 5000000)
                            .ToList()
                        : b.Branches
                            .Where(branch =>
                                branch.IsActive &&
                                branch.CityId == userCityId)
                            .ToList();

                    if (!matchingBranches.Any()) continue;

                    foreach (var branch in matchingBranches)
                    {
                        var item = _mapper.Map<BusinessLocationItem>(b);
                        item.Name = b.GetNameByLanguageCode(languageCode);
                        item.BranchInfo = _mapper.Map<BranchLocationItem>(branch);

                        if (userLatitude.HasValue && userLongitude.HasValue)
                        {
                            var kmAway = LocationHelper.CalculateDistance(
                                   (userLatitude.Value, userLongitude.Value),
                                   (branch.Latitude, branch.Longitude)
                               );
                            item.BranchInfo.KmAway = kmAway;
                        }
                        Items.Add(item);
                    }
                }


                var items = Items.OrderBy(o => o.BranchInfo.KmAway).Take(25).ToList();
                var totalItems = items.Count;

                var payload = new BusinessByLocationPayload
                {
                    Items = items,
                    TotalItems = totalItems,
                    HasNextPage = false
                };


                return new GetBusinessByLocationResponse(
                    StatusCodes.Status200OK,
                    "Get Branches",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting business");
                return new GetBusinessByLocationResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }


        public async Task<SuccessResponse> SetBusinessAppLinks(Entities.BusinessUser business, SetBusinessAppLinksDTO model)
        {
            try
            {
                business.AndroidAppUrl = model.AndroidAppUrl;
                business.iOSAppUrl = model.iOSAppUrl;
                await _businessUserRepository.UpdateAsync(business, true);

                return new SuccessResponse(
                    StatusCodes.Status200OK,
                    $"App link Update Success",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting app links");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> SetBusinessWebiste(Entities.BusinessUser business, SetWebsiteLinkDTO model)
        {
            try
            {
                business.WebsiteURL = model.WebsiteURL;
                await _businessUserRepository.UpdateAsync(business, true);

                return new SuccessResponse(
                    StatusCodes.Status200OK,
                    $"Website Update Success",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting app Url");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetBusinessAppResponse> GetBusinessAppLinks(Guid businessId)
        {
            try
            {
                var business = await _businessUserRepository.GetByIdAsync(businessId);
                if (business == null)
                {
                    return new GetBusinessAppResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Entity with Id {businessId} not found",
                        payload: null
                    );
                }

                var payload = new GetBusinessAppPayload
                {
                    AndroidAppUrl = business.AndroidAppUrl,
                    iOSAppUrl = business.iOSAppUrl,
                };

                return new GetBusinessAppResponse(
                    StatusCodes.Status200OK,
                    $"Get Business AppLinks Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting app Url");
                return new GetBusinessAppResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBusinessWebisteResponse> GetBusinessWebiste(Guid businessId)
        {
            try
            {
                var business = await _businessUserRepository.GetByIdAsync(businessId);
                if (business == null)
                {
                    return new GetBusinessWebisteResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Entity with Id {businessId} not found",
                        payload: null
                    );
                }

                var payload = new GetBusinessWebistePayload
                {
                    WebsiteURL = business.WebsiteURL
                };

                return new GetBusinessWebisteResponse(
                    StatusCodes.Status200OK,
                    $"Get Business Webiste Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting webiste");
                return new GetBusinessWebisteResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBusinessDetailsById> GetBusinessDetailsById(Guid userId, Guid businessId, LocationDTO? location)
        {
            try
            {
                var response = await _businessUserRepository.GetBusinessWithRelation(businessId);
                if (response == null)
                {
                    return new GetBusinessDetailsById
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: $"Entity with Id {businessId} not found",
                        payload: null
                    );
                }

                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBusinessDetailsById
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var howToUse = await _policyRepository.GetPolicyByType(PolicyType.HowToUse);

                var payload = _mapper.Map<GetBusinessDetailsPayload>(response);
                payload.Name = response.GetNameByLanguageCode(languageCode);
                if (howToUse != null)
                {
                    payload.HowToUse = howToUse!.GetContentByLanguageCode(languageCode);
                }

                if (location != null)
                {
                    var nearestBranch = response.GetNearestBranchWithDistance(location.Latitude, location.Longitude);
                    if (nearestBranch.Branch != null)
                    {
                        var (Rating, Review) = (nearestBranch.Branch.Rating, nearestBranch.Branch.ReviewCount);
                        payload.Rating = Rating;
                        payload.Review = Review;
                    }
                }

                var isFavorite = await _favoriteRepository.IsFavourite(userId, businessId);
                payload.Favourite = isFavorite;

                var offers = await _offerRepository.GetBusinessOffers(businessId, true);
                payload.Offers = [];
                foreach (var offer in offers)
                {
                    var o = _mapper.Map<GetBusinessOffersDetails>(offer);
                    o.OfferUrl = offer.Logos.Select(x => x.LogoUrl).ToList();
                    payload.Offers.Add(o);
                }

                var branches = await _branchRepository.GetBusinessBranches(businessId);
                payload.Branches = [];
                foreach (var branch in branches)
                {
                    var b = _mapper.Map<GetBusinessBranchDetails>(branch);
                    if (location != null)
                    {
                        b.KmAway = branch.GetBranchDistance(location.Latitude, location.Longitude);
                    }
                    payload.Branches.Add(b);
                }

                var termsAndConditions = await _businessPolicyRepository.GetBusinessPolicyByType(businessId, PolicyType.TermsAndConditions);
                if (termsAndConditions != null)
                {
                    payload.TermsAndConditions = _mapper.Map<PolicyLocalized>(termsAndConditions);
                    payload.TermsAndConditions.SetPolicyContentLocalized(termsAndConditions.GetContentByLanguageCode(languageCode));
                }

                var privacyPolicy = await _businessPolicyRepository.GetBusinessPolicyByType(businessId, PolicyType.PrivacyPolicy);
                if (privacyPolicy != null)
                {
                    payload.PrivacyPolicy = _mapper.Map<PolicyLocalized>(privacyPolicy);
                    payload.PrivacyPolicy.SetPolicyContentLocalized(privacyPolicy.GetContentByLanguageCode(languageCode));
                }

                return new GetBusinessDetailsById
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Business Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching business user with id : " + businessId);
                return new GetBusinessDetailsById
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

                var query = _businessUserRepository.GetBusinessForFilter()
                    .Where(b => b.IsActive)
                    .Where(b => b.Branches.Count > 0);

                // Filter by category
                if (!string.IsNullOrEmpty(model.CategoryId) && int.TryParse(model.CategoryId, out int categoryId) && categoryId != 0)
                {
                    query = query.Where(b => b.CategoryId == categoryId);
                }

                // Filter by search keyword
                if (!string.IsNullOrEmpty(model.searchKeyword))
                {
                    var searchKeyword = model.searchKeyword.ToLower();
                    query = languageCode == Language.en
                        ? query.Where(b => b.NameEn.ToLower().Contains(searchKeyword))
                        : query.Where(b => b.NameAr.ToLower().Contains(searchKeyword));
                }

                // Apply sorting at database level
                query = model.SortBySlug switch
                {
                    "newest" => query.OrderByDescending(b => b.CreationTime),
                    "oldest" => query.OrderBy(b => b.CreationTime),
                    "a-z" => query.OrderBy(b => b.NameEn),
                    "z-a" => query.OrderByDescending(b => b.NameEn),
                    _ => query
                };

                // Fetch all items first before filtering
                var businessList = await query
                    .Include(b => b.Branches) // Include branches
                    .Include(b => b.Offers)   // Include offers for discount filtering
                    .ToListAsync();

                if (model.SortBySlug == "top-rating")
                {
                    var items = await query.ToListAsync();
                    List<(Entities.BusinessUser business, double rating)> businessUsers = [];

                    foreach (var b in businessList)
                    {
                        var (Branch, Distance) = b.GetNearestBranchWithDistance(model.Latitude, model.Longitude);
                        var (Rating, Review) = (0D, 0);
                        if (Branch != null)
                        {
                            (Rating, Review) = (Branch.Rating, Branch.ReviewCount);
                        }
                        businessUsers.Add((b, Rating));
                    }

                    businessList = businessUsers.OrderByDescending(x => x.rating).Select(x => x.business).ToList();
                }

                // Apply in-memory filtering (for non-database filters)
                if (!string.IsNullOrEmpty(model.DistanceSlug) &&
                    TryParseDistance(model.DistanceSlug, out double maxDistance) &&
                    model.Latitude.HasValue && model.Longitude.HasValue)
                {
                    businessList = businessList
                        .Where(b => b.Branches.Any(branch =>
                            branch.IsActive &&
                            branch.Latitude != 0 && branch.Longitude != 0 &&
                            CalculateDistance(
                                model.Latitude.Value,
                                model.Longitude.Value,
                                branch.Latitude,
                                branch.Longitude
                            ) <= maxDistance
                        ))
                        .ToList();
                }

                if (!string.IsNullOrEmpty(model.DiscountSlug) && TryParseDiscount(model.DiscountSlug, out int discountThreshold))
                {
                    businessList = businessList
                        .Where(b => b.Offers.Any(o => o.PercentageValue >= discountThreshold || o.InvoiceValue >= discountThreshold))
                        .ToList();
                }

                // Get total count after filtering
                var totalItems = businessList.Count;

                // 🔥 Apply pagination at the end!
                var paginatedList = businessList
                    .Skip((model.pageNumber - 1) * model.pageSize)
                    .Take(model.pageSize)
                    .ToList();

                // Check if there's a next page
                bool hasNextPage = (model.pageNumber * model.pageSize) < totalItems;

                var payload = new CategoryItemPayload
                {
                    Type = "Business",
                    Items = [],
                    TotalItems = paginatedList.Count,
                    HasNextPage = hasNextPage
                };

                foreach (var b in paginatedList)
                {
                    var (Branch, Distance) = b.GetNearestBranchWithDistance(model.Latitude, model.Longitude);
                    var (Rating, Review) = (0D, 0);
                    if (Branch != null)
                    {
                        (Rating, Review) = (Branch.Rating, Branch.ReviewCount);
                    }

                    payload.Items.Add(new BusinessTypeItem
                    {
                        id = b.Id,
                        Name = b.GetNameByLanguageCode(languageCode),
                        Category = b.Category.GetNameByLanguageCode(languageCode),
                        Logo = b.LogoPath,
                        KmAway = Distance,
                        Rating = Rating,
                        Review = Review
                    });
                }

                return new GetFilterByTypeResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Business Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching businesses");
                return new GetFilterByTypeResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Radius of Earth in kilometers
            var latDistance = DegreesToRadians(lat2 - lat1);
            var lonDistance = DegreesToRadians(lon2 - lon1);

            var a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(lonDistance / 2) * Math.Sin(lonDistance / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        private bool TryParseDistance(string slug, out double distance)
        {
            return double.TryParse(slug.Replace("km", ""), out distance);
        }

        private bool TryParseDiscount(string slug, out int discount)
        {
            return int.TryParse(slug.Replace("%", ""), out discount);
        }

        public async Task<GetBusinessDetailsById> GetBusinessDetailsByBranch(Guid userId, int branchId, LocationDTO? location)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBusinessDetailsById
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var branch = await _branchRepository.GetBranchWithRelations(branchId);
                if (branch == null)
                {
                    return new GetBusinessDetailsById
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: $"Entity with Id {branch} not found",
                        payload: null
                    );
                }
                var business = await _businessUserRepository.GetBusinessWithRelation(branch.BusinessId);
                if (business == null)
                {
                    return new GetBusinessDetailsById
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: $"Entity with Id {branch.BusinessId} not found",
                        payload: null
                    );
                }

                var howToUse = await _policyRepository.GetPolicyByType(PolicyType.HowToUse);

                var payload = _mapper.Map<GetBusinessDetailsPayload>(business);
                payload.Name = business.GetNameByLanguageCode(languageCode);
                if (howToUse != null)
                {
                    payload.HowToUse = howToUse!.GetContentByLanguageCode(languageCode);
                }

                var isFavorite = await _favoriteRepository.IsFavourite(userId, business.Id);
                payload.Favourite = isFavorite;

                var offers = await _offerRepository.GetBusinessOffers(business.Id, true);
                payload.Offers = [];
                foreach (var offer in offers)
                {
                    var o = _mapper.Map<GetBusinessOffersDetails>(offer);
                    o.OfferUrl = offer.Logos.Select(x => x.LogoUrl).ToList();
                    payload.Offers.Add(o);
                }

                payload.Branches = [];
                var b = _mapper.Map<GetBusinessBranchDetails>(branch);
                if (location != null)
                {
                    b.KmAway = branch.GetBranchDistance(location.Latitude, location.Longitude);
                }
                payload.Branches.Add(b);

                var termsAndConditions = await _businessPolicyRepository.GetBusinessPolicyByType(business.Id, PolicyType.TermsAndConditions);
                if (termsAndConditions != null)
                {
                    payload.TermsAndConditions = _mapper.Map<PolicyLocalized>(termsAndConditions);
                    payload.TermsAndConditions.SetPolicyContentLocalized(termsAndConditions.GetContentByLanguageCode(languageCode));
                }

                var privacyPolicy = await _businessPolicyRepository.GetBusinessPolicyByType(business.Id, PolicyType.PrivacyPolicy);
                if (privacyPolicy != null)
                {
                    payload.PrivacyPolicy = _mapper.Map<PolicyLocalized>(privacyPolicy);
                    payload.PrivacyPolicy.SetPolicyContentLocalized(privacyPolicy.GetContentByLanguageCode(languageCode));
                }

                payload.Rating = branch.Rating;
                payload.Review = branch.ReviewCount;

                return new GetBusinessDetailsById
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Business Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching business user with id : " + branchId);
                return new GetBusinessDetailsById
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }
    }
}