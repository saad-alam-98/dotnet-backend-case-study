using AutoMapper;
using SaudiPlus.Enums;
using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Entities;
using SaudiPlus.Extensions;
using SaudiPlus.Interfaces;
using SaudiPlus.Response.User;
using SaudiPlus.Helper.Bucket;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Account;
using SaudiPlus.Entities.Identity;
using Microsoft.Extensions.Options;
using SaudiPlus.Interfaces.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SaudiPlus.Interfaces.Repository;
using SaudiPlus.Request.IndividualUser;
using SaudiPlus.Response.IndividualUser;

namespace SaudiPlus.Service.IndividualUser
{
    public class IndividualUserService : BaseService<IndividualUserService>, IIndividualUserService
    {
        private readonly BucketSetting _bucket;
        private readonly IMapService _mapService;
        private readonly IFileService _fileService;
        private readonly ITokenService _tokenService;
        private readonly SaudiPlusDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ICityRepository _cityRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IFavouriteRepository _favoriteRepository;
        private readonly INotificationService _notificationService;
        private readonly IIndividualUserRepository _individualUserRepository;

        public IndividualUserService(
            IMapService mapService,
            IFileService fileService,
            ITokenService tokenService,
            SaudiPlusDbContext dbContext,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            ICityRepository cityRepository,
            IOptions<BucketSetting> bucket,
            ILanguageService languageService,
            IInvoiceRepository invoiceRepository,
            ILogger<IndividualUserService> logger,
            IFavouriteRepository favoriteRepository,
            INotificationService notificationService,
            IIndividualUserRepository individualUserRepository,
            IMapper mapper
        ) : base(mapper, logger, languageService)
        {
            _dbContext = dbContext;
            _bucket = bucket.Value;
            _mapService = mapService;
            _roleManager = roleManager;
            _fileService = fileService;
            _userManager = userManager;
            _tokenService = tokenService;
            _cityRepository = cityRepository;
            _invoiceRepository = invoiceRepository;
            _favoriteRepository = favoriteRepository;
            _notificationService = notificationService;
            _individualUserRepository = individualUserRepository;
        }

        public async Task<RegisterUserResponse> RegisterUser(RegisterUserDTO model)
        {
            if (model.CityId.HasValue && model.CityId.Value > 0)
            {
                var city = await _cityRepository.GetByIdAsync(model.CityId.Value);
                if (city == null)
                {
                    return new RegisterUserResponse
                   (
                       statusCode: StatusCodes.Status400BadRequest,
                       message: "Please enter valid city id",
                       payload: null
                   );
                }
            }
            else
            {
                model.CityId = null;
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var IndividualUser = await _individualUserRepository.GetUserByUserId(user.Id);
                if (IndividualUser != null && IndividualUser.IsDelete)
                {
                    IndividualUser.IsDelete = false;
                    await _individualUserRepository.UpdateAsync(IndividualUser, true);
                    var token = await _tokenService.GenerateAccessToken(user);

                    return new RegisterUserResponse(
                        statusCode: StatusCodes.Status200OK,
                        message: "Login Success",
                        payload: new IndividualUserLoginPayload
                        {
                            AccessToken = token.accessToken,
                            RefreshToken = token.refreshToken,
                            DeviceToken = IndividualUser.DeviceToken ?? "",
                            IsPhoneConfirmed = user.PhoneNumberConfirmed,
                            Expiry = token.expiry,
                        }
                    );
                }
            }

            var error = await _userManager.ValidateEmailUniqueness(model.Email);
            if (error != null)
            {
                return new RegisterUserResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: error.Message,
                    payload: null
                );
            }

            if (model.Password != model.ConfirmPassword)
            {
                return new RegisterUserResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "Password does not match",
                    payload: null
                );
            }

            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var newUser = _mapper.Map<User>(model);
                    newUser.UserName = string.Concat(model.Email.Split('@')[0], "_", Guid.NewGuid().ToString("N").AsSpan(0, 6));

                    var createResult = await _userManager.CreateAsync(newUser, model.Password);

                    if (!createResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return new RegisterUserResponse
                        (
                            statusCode: StatusCodes.Status500InternalServerError,
                            message: string.Join(", ", createResult.Errors.Select(e => e.Description)),
                            payload: null
                        );
                    }

                    await _roleManager.EnsureRoleExistsAsync(UserRole.User.ToString(), newUser.Id);
                    await _userManager.AddUserToRole(newUser, UserRole.User.ToString());

                    var IndividualUser = _mapper.Map<Entities.IndividualUser>(model);

                    IndividualUser.UserId = newUser.Id;
                    IndividualUser.IsActive = true;
                    IndividualUser.LoginType = LoginType.Email.ToString();

                    await _individualUserRepository.InsertAsync(IndividualUser, true);
                    var token = await _tokenService.GenerateAccessToken(newUser);

                    await transaction.CommitAsync();

                    return new RegisterUserResponse
                    (
                        statusCode: StatusCodes.Status200OK,
                        message: "Individual User Created",
                        payload: new IndividualUserLoginPayload
                        {
                            AccessToken = token.accessToken,
                            RefreshToken = token.refreshToken,
                            DeviceToken = IndividualUser.DeviceToken ?? "",
                            IsPhoneConfirmed = newUser.PhoneNumberConfirmed,
                            Expiry = token.expiry,
                        }
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while creating an individual user");
                    return new RegisterUserResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "An unexpected error occurred. Please try again later.",
                        payload: null
                    );
                }
            });
        }

        public async Task<UserCreateResponse> CreateIndividualUser(CreateUserDTO model)
        {
            var (languageCode, languageError) = _languageService.GetLanguageCode();
            if (languageError != null)
            {
                return new UserCreateResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: languageError.Message,
                    payload: null
                );
            }

            var city = await _cityRepository.GetByIdAsync(model.cityId);
            if (city == null)
            {
                return new UserCreateResponse
               (
                   statusCode: StatusCodes.Status400BadRequest,
                   message: "Please enter valid city id",
                   payload: null
               );
            }

            var user = await _userManager.FindByEmailAsync(model.email);
            if (user != null)
            {
                var IndividualUser = await _individualUserRepository.GetUserByUserId(user.Id);
                if (IndividualUser != null && IndividualUser.IsDelete)
                {
                    IndividualUser.IsDelete = false;
                    await _individualUserRepository.UpdateAsync(IndividualUser, true);

                    var payload = _mapper.Map<IndividualUserData>(IndividualUser);
                    payload.SetCityNameLocalized(IndividualUser.GetCityNameLocalized(languageCode));

                    return new UserCreateResponse
                    (
                        statusCode: StatusCodes.Status200OK,
                        message: "Individual User Created",
                        payload: payload
                    );
                }
            }

            var error = await _userManager.ValidateEmailUniqueness(model.email);
            if (error != null)
            {
                return new UserCreateResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: error.Message,
                    payload: null
                );
            }

            model.username = string.IsNullOrEmpty(model.username) ?
                model.email.Split('@')[0] + "_" + Guid.NewGuid().ToString("N").Substring(0, 6) :
                model.username;

            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var newUser = _mapper.Map<User>(model);
                    var createResult = await _userManager.CreateAsync(newUser, model.password);

                    if (!createResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return new UserCreateResponse
                        (
                            statusCode: StatusCodes.Status500InternalServerError,
                            message: string.Join(", ", createResult.Errors.Select(e => e.Description)),
                            payload: null
                        );
                    }

                    await _roleManager.EnsureRoleExistsAsync(UserRole.User.ToString(), newUser.Id);
                    await _userManager.AddUserToRole(newUser, UserRole.User.ToString());

                    var IndividualUser = _mapper.Map<Entities.IndividualUser>(model);

                    IndividualUser.UserId = newUser.Id;
                    IndividualUser.IsActive = true;
                    IndividualUser.LoginType = LoginType.Email.ToString();

                    await _individualUserRepository.InsertAsync(IndividualUser, true);
                    var payload = _mapper.Map<IndividualUserData>(IndividualUser);
                    payload.SetCityNameLocalized(IndividualUser.GetCityNameLocalized(languageCode));

                    await transaction.CommitAsync();

                    return new UserCreateResponse
                    (
                        statusCode: StatusCodes.Status200OK,
                        message: "Individual User Created",
                        payload: payload
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while creating an individual user");
                    return new UserCreateResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "An unexpected error occurred. Please try again later.",
                        payload: null
                    );
                }
            });
        }

        public async Task<IndividualUserLoginResponse> LoginUser(LoginRequestDTO model)
        {
            var (languageCode, languageError) = _languageService.GetLanguageCode();
            if (languageError != null)
            {
                return new IndividualUserLoginResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: languageError.Message,
                    payload: null
                );
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                var error = _userManager.ErrorDescriber.InvalidEmail(model.Email);
                return new IndividualUserLoginResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: error.Description,
                    payload: null
                );
            }

            var isValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isValid)
            {
                var error = _userManager.ErrorDescriber.PasswordMismatch();
                return new IndividualUserLoginResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: error.Description,
                    payload: null
                );
            }

            var existingUser = await _individualUserRepository.GetUserByUserId(user.Id);
            if (existingUser == null)
            {
                return new IndividualUserLoginResponse(
                    statusCode: StatusCodes.Status400BadRequest,
                    message: $"Entity with Id {user.Id} not found",
                    payload: null
                );
            }

            if (existingUser.IsDelete || !existingUser.IsActive)
            {
                return new IndividualUserLoginResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "User not found",
                    payload: null
                );
            }

            try
            {
                // Force update security stamp to invalidate previous sessions
                await _userManager.UpdateSecurityStampAsync(user);
                
                var token = await _tokenService.GenerateAccessToken(user);

                return new IndividualUserLoginResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "Login Success",
                    payload: new IndividualUserLoginPayload
                    {
                        AccessToken = token.accessToken,
                        RefreshToken = token.refreshToken,
                        DeviceToken = existingUser.DeviceToken ?? "",
                        IsPhoneConfirmed = user.PhoneNumberConfirmed,
                        Expiry = token.expiry,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in");
                return new IndividualUserLoginResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<IndividualUserLoginResponse> GuestUserLogin(GuestLoginDTO model)
        {
            var (languageCode, languageError) = _languageService.GetLanguageCode();
            if (languageError != null)
            {
                return new IndividualUserLoginResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: languageError.Message,
                    payload: null
                );
            }

            if (string.IsNullOrEmpty(model.DeviceIdentifier))
            {
                return new IndividualUserLoginResponse(
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "Invalid Identifier",
                    payload: null
                );
            }

            var city = await _cityRepository.GetByIdAsync(model.CityId);
            if (city == null)
            {
                return new IndividualUserLoginResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "Please enter valid city id",
                    payload: null
                );
            }

            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    IndividualUserData individualUserData = null;
                    var guestUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.DeviceIdentifier == model.DeviceIdentifier && u.IsGuest);

                    if (guestUser == null)
                    {
                        guestUser = new User
                        {
                            UserName = $"guest_{Guid.NewGuid()}",
                            IsGuest = true,
                            DeviceIdentifier = model.DeviceIdentifier,
                            Email = $"{Guid.NewGuid()}@guest.local"
                        };

                        var createResult = await _userManager.CreateAsync(guestUser);

                        if (!createResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return new IndividualUserLoginResponse
                            (
                                statusCode: StatusCodes.Status500InternalServerError,
                                message: string.Join(", ", createResult.Errors.Select(e => e.Description)),
                                payload: null
                            );
                        }

                        await _roleManager.EnsureRoleExistsAsync(UserRole.User.ToString(), guestUser.Id);
                        await _userManager.AddUserToRole(guestUser, UserRole.User.ToString());

                        var IndividualUser = _mapper.Map<Entities.IndividualUser>(model);

                        var random = new Random();
                        int randomNumber = random.Next(1000, 10000);

                        IndividualUser.Email = guestUser.Email;
                        IndividualUser.DisplayName = $"Guest#{randomNumber}";
                        IndividualUser.PhoneNumber = "";
                        IndividualUser.Address = "";
                        IndividualUser.Gender = "Male";
                        IndividualUser.UserId = guestUser.Id;
                        IndividualUser.IsActive = true;
                        IndividualUser.LoginType = LoginType.Guest.ToString();

                        await _individualUserRepository.InsertAsync(IndividualUser, true);
                        individualUserData = _mapper.Map<IndividualUserData>(IndividualUser);
                        individualUserData.SetCityNameLocalized(IndividualUser.GetCityNameLocalized(languageCode));
                    }

                    var token = await _tokenService.GenerateAccessToken(guestUser);

                    await transaction.CommitAsync();
                    return new IndividualUserLoginResponse(
                        statusCode: StatusCodes.Status200OK,
                        message: "Login Success",
                        payload: new IndividualUserLoginPayload
                        {
                            AccessToken = token.accessToken,
                            RefreshToken = token.refreshToken,
                            DeviceToken = "",
                            IsPhoneConfirmed = guestUser.PhoneNumberConfirmed,
                            Expiry = token.expiry
                        }
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while creating an individual user");
                    return new IndividualUserLoginResponse(
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "An unexpected error occurred. Please try again later.",
                        payload: null
                    );
                }
            });
        }

        public async Task<UserEditResponse> EditUser(Guid userID, EditUserDTO model)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new UserEditResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var existingUser = await _individualUserRepository.GetUserWithRelations(userID);
                if (existingUser == null)
                {
                    return new UserEditResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Entity with Id {userID} not found",
                        payload: null
                    );
                }

                var user = await _userManager.FindByIdAsync(existingUser.UserId.ToString());
                if (user == null)
                {
                    return new UserEditResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"User with Id {userID} not found",
                        payload: null
                    );
                }

                if (model.CityId != null)
                {
                    var city = await _cityRepository.GetByIdAsync(model.CityId.Value);
                    if (city == null)
                    {
                        return new UserEditResponse
                        (
                            statusCode: StatusCodes.Status400BadRequest,
                            message: $"City with Id {model.CityId} not found",
                            payload: null
                        );
                    }
                }

                //await _individualUserRepository.BeginTransactionAsync();

                _mapper.Map(model, user);
                await _userManager.UpdateAsync(user);

                model.IsActive = model.IsActive.HasValue ? model.IsActive.Value : existingUser.IsActive;
                _mapper.Map(model, existingUser);

                await _individualUserRepository.UpdateAsync(existingUser, true);

                var payload = _mapper.Map<IndividualUserData>(existingUser);
                payload.SetCityNameLocalized(existingUser.GetCityNameLocalized(languageCode));
                //await _individualUserRepository.CommitTransactionAsync();

                return new UserEditResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "User Updated",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                //await _individualUserRepository.RollbackTransactionAsync();
                _logger.LogError(ex, "An error occurred editing user");

                return new UserEditResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> ToggleUserStatus(ToggleStatusDTO<Guid> model)
        {
            try
            {
                var individualUser = await _individualUserRepository.GetUserWithRelations(model.Id);
                if (individualUser == null)
                {
                    return new SuccessResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Entity with Id {model.Id} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                //await _individualUserRepository.BeginTransactionAsync();
                individualUser.IsActive = model.IsActive;
                await _individualUserRepository.UpdateAsync(individualUser, true);
                //await _individualUserRepository.CommitTransactionAsync();

                return new SuccessResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "User Updated",
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

        public async Task<GetUserResponse> GetUserById(Guid userId)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetUserResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var individualUser = await _individualUserRepository.GetUserWithRelations(userId);
                if (individualUser == null)
                {
                    return new GetUserResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Entity with Id {userId} not found",
                        payload: null
                    );
                }

                //await _individualUserRepository.BeginTransactionAsync();

                var payload = _mapper.Map<IndividualUserData>(individualUser);
                payload.SetCityNameLocalized(individualUser.GetCityNameLocalized(languageCode));
                //await _individualUserRepository.CommitTransactionAsync();

                return new GetUserResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "User Updated",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                //await _individualUserRepository.RollbackTransactionAsync();
                _logger.LogError(ex, "An error occurred editing user");
                return new GetUserResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetUsersResponse> GetUserByFilter(EntityFilter paginationFilter)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetUsersResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var query = _individualUserRepository.GetUsersForFilter();
                if (paginationFilter.isActive.HasValue)
                {
                    query = query
                        .Where(user => user.IsActive == paginationFilter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(paginationFilter.searchKeyword))
                {
                    var searchKeyword = paginationFilter.searchKeyword.ToLower();
                    query = query.Where(user =>
                        user.Email.ToLower().Contains(searchKeyword) ||
                        user.Address.ToLower().Contains(searchKeyword) ||
                        user.DisplayName.ToLower().Contains(searchKeyword));
                }

                var totalItems = await query.CountAsync();
                var businesses = await query
                    .Skip((paginationFilter.pageNumber - 1) * paginationFilter.pageSize)
                    .Take(paginationFilter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (paginationFilter.pageNumber * paginationFilter.pageSize) < totalItems;

                var payload = new GetUserPayload
                {
                    Items = businesses.Select(b =>
                    {
                        var item = _mapper.Map<IndividualUserData>(b);
                        item.SetCityNameLocalized(b.GetCityNameLocalized(languageCode));
                        return item;
                    }).ToList(),
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                return new GetUsersResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Users Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching user");
                return new GetUsersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> DeleteUser(Guid userId)
        {
            try
            {
                var individualUser = await _individualUserRepository.GetByIdAsync(userId);
                if (individualUser == null)
                {
                    return new SuccessResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Entity with Id {userId} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                var user = await _userManager.FindByIdAsync(individualUser.UserId.ToString());
                if (user == null)
                {
                    return new SuccessResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"User with Id {userId} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }
                //await _individualUserRepository.BeginTransactionAsync();
                await _userManager.DeleteAsync(user);
                await _individualUserRepository.DeleteAsync(individualUser!.Id, true);
                //await _individualUserRepository.CommitTransactionAsync();

                return new SuccessResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "User Deleted",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an individual user");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> SetAvatar(Entities.IndividualUser user, SetUserAvatarDTO model)
        {
            try
            {
                if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    await _fileService.DeleteFileAsync(user.AvatarUrl);
                }

                var uploadResponse = await _fileService.UploadFileAsync(
                    model.Avatar,
                    _bucket.Avatar + "/" + user.Id
                );

                if (uploadResponse.error != null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: uploadResponse.error.Message,
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                user.AvatarUrl = uploadResponse.URL;
                await _individualUserRepository.UpdateAsync(user, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Upload Success",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while uploading avatar");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetOrdersResponse> GetOrders(Guid userID, EntityFilter filter)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetOrdersResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var query = _invoiceRepository.GetUserInvoiceForFilter(userID);

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

                var payload = new GetUserOrdersPayload
                {
                    Items = invoices.Select(o =>
                    {
                        var item = _mapper.Map<GetUserOrdersItems>(o);
                        item.OrderNumber = o.InvoiceNumber;
                        return item;
                    }).ToList(),
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                return new GetOrdersResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Get Orders Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting orders");
                return new GetOrdersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetPointsResponse> GetPoints(Entities.IndividualUser user)
        {
            try
            {
                var payload = _mapper.Map<GetUserPointsPayload>(user);
                return new GetPointsResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Points",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting point");
                return new GetPointsResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetAvatarResponse> GetAvatar(Entities.IndividualUser user)
        {
            try
            {
                var payload = _mapper.Map<GetAvatarPayload>(user);
                return new GetAvatarResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Upload Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting avatar");
                return new GetAvatarResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> SetDeviceToken(Entities.IndividualUser user, string deviceToken)
        {
            try
            {
                if (user.DeviceToken != deviceToken)
                {
                    var DeviceArn = await _notificationService.CreatePlatformEndpoint(deviceToken);
                    user.DeviceToken = deviceToken;
                    user.DeviceArn = DeviceArn;

                    await _individualUserRepository.UpdateAsync(user, true);
                }

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Upload Success",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting device token");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> SetLanguage(Entities.IndividualUser user, SetLanguageDTO model)
        {
            try
            {
                if (!Enum.TryParse<Language>(model.Language, true, out var languageCode))
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Invalid language code",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                user.Language = languageCode;
                await _individualUserRepository.UpdateAsync(user, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Upload Success",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting language");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> ToggleFavourite(Guid userId, Guid businessUserId, bool isFavourite)
        {
            try
            {
                var alreadyFavourite = await _favoriteRepository.GetFavourite(userId, businessUserId);
                if (isFavourite && alreadyFavourite == null)
                {
                    var favorite = new Favourite
                    {
                        BusinessUserId = businessUserId,
                        UserId = userId
                    };
                    await _favoriteRepository.InsertAsync(favorite, true);
                }
                else if (!isFavourite && alreadyFavourite != null)
                {
                    await _favoriteRepository.DeleteAsync(alreadyFavourite.Id, true);
                }

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Upload Success",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while toggling favorite");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetFavouriteBusinessResponse> GetFavouriteBusiness(Guid userId, LocationDTO? location)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetFavouriteBusinessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var favourites = await _favoriteRepository.GetFavouriteBusiness(userId);
                var payload = new GetFavouriteBusinessPayload
                {
                    Items = []
                };

                foreach (var favourite in favourites)
                {
                    var (Branch, Distance) = favourite.GetNearestBranchWithDistance(location.Latitude, location.Longitude);
                    var (Rating, Review) = (0D, 0);
                    if (Branch != null)
                    {
                        (Rating, Review) = (Branch.Rating, Branch.ReviewCount);
                    }

                    var item = _mapper.Map<FavouriteBusinessItem>(favourite);
                    item.Name = favourite.GetNameByLanguageCode(languageCode);
                    item.Rating = Rating;
                    item.Review = Review;
                    payload.Items.Add(item);
                }

                return new GetFavouriteBusinessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Get favourite business response",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting favorites");
                return new GetFavouriteBusinessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> DeleteAccount(Entities.IndividualUser user)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var identity = await _userManager.FindByEmailAsync(user.Email);
                    if (identity == null)
                    {
                        await transaction.RollbackAsync();
                        var error = _userManager.ErrorDescriber.InvalidEmail(user.Email);
                        return new SuccessResponse(
                            statusCode: StatusCodes.Status500InternalServerError,
                            message: error.Description,
                            payload: new SuccessResponsePayload { success = false }
                        );
                    }

                    user.IsDelete = true;
                    await _individualUserRepository.UpdateAsync(user, true);

                    await transaction.CommitAsync();
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status200OK,
                        message: "Upload Success",
                        payload: new SuccessResponsePayload { success = true }
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while setting language");
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "An unexpected error occurred. Please try again later.",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }
            });
        }
    }
}
