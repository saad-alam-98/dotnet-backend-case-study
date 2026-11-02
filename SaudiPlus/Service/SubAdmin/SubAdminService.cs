using AutoMapper;
using SaudiPlus.Enums;
using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Entities;
using SaudiPlus.Constants;
using SaudiPlus.Interfaces;
using SaudiPlus.Extensions;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.SubAdmin;
using SaudiPlus.Entities.Identity;
using SaudiPlus.Response.SubAdmin;
using SaudiPlus.Interfaces.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Service.SubAdmin
{
    public class SubAdminService : BaseService<SubAdminService>, ISubAdminService
    {
        private readonly SaudiPlusDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ISubAdminRepository _subAdminRepository;
        private readonly IUserPermissionRepository _userPermissionRepository;

        public SubAdminService(
            IMapper mapper,
            SaudiPlusDbContext dbContext,
            RoleManager<Role> roleManager,
            UserManager<User> userManager,
            ILogger<SubAdminService> logger,
            ILanguageService languageService,
            ISubAdminRepository subAdminRepository,
            IUserPermissionRepository userPermissionRepository
        ) : base(mapper, logger, languageService)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _subAdminRepository = subAdminRepository;
            _userPermissionRepository = userPermissionRepository;
        }

        public async Task<GetUserPermissionsResponse> GetUserPermissions()
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetUserPermissionsResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var grouped = Permissions.All()
                    .Select(group => new Response.SubAdmin.PermissionGroup
                    {
                        Group = languageCode == Language.ar ? group.GroupAr : group.GroupEn,
                        Items = group.Permissions.Select(p => new UserPermissionItem
                        {
                            Id = p.Id,
                            Name = languageCode == Language.ar ? p.NameAr : p.NameEn
                        }).ToList()
                    })
                    .ToList();

                return new GetUserPermissionsResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "Permissions fetched successfully.",
                    payload: new GetUserPermissionsPayload
                    {
                        Permissions = grouped
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching permissions");
                return new GetUserPermissionsResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> CreateSubAdminUser(CreateSubAdminUserDTO model)
        {
            var existingUser = await _subAdminRepository.GetSubAdminUserByEmail(model.Email);
            if (existingUser != null && existingUser.IsDelete == true)
            {
                existingUser.IsDelete = false;
                await _subAdminRepository.UpdateAsync(existingUser, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Success",
                    payload: new SuccessResponsePayload { success = true }
                );
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "User already exist",
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
                    newUser.UserName = model.Username;
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

                    await _roleManager.EnsureRoleExistsAsync(UserRole.SubAdmin.ToString(), newUser.Id);
                    await _userManager.AddUserToRole(newUser, UserRole.SubAdmin.ToString());

                    var subAdminUser = _mapper.Map<SubAdminUser>(model);
                    subAdminUser.UserId = newUser.Id;
                    subAdminUser.IsActive = true;

                    await _subAdminRepository.InsertAsync(subAdminUser, true);

                    var permissions = model.Permissions.Select(pid => new UserPermission
                    {
                        IsActive = true,
                        UserId = subAdminUser.UserId,
                        PermissionId = pid
                    }).ToArray();

                    await _userPermissionRepository.InsertAsync(permissions, true);
                    await transaction.CommitAsync();

                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status200OK,
                        message: "Success",
                        payload: new SuccessResponsePayload { success = true }
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while creating an subdmin user");
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "An unexpected error occurred. Please try again later.",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }
            });
        }

        public async Task<SuccessResponse> EditSubAdminUser(EditSubAdminUserDTO model)
        {

            var subAdminUser = await _subAdminRepository.GetSubAdminUserByRelation(model.Id);
            if (subAdminUser == null)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: $"Entity with Id {model.Id} not found",
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            var identityUser = await _userManager.FindByIdAsync(subAdminUser.UserId.ToString());
            if (identityUser == null)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: $"Entity with Id {model.Id} not found",
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {

                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        foreach (var validator in _userManager.PasswordValidators)
                        {
                            var validationResult = await validator.ValidateAsync(_userManager, identityUser, model.Password);
                            if (!validationResult.Succeeded)
                            {
                                await transaction.RollbackAsync();
                                var errors = string.Join(", ", validationResult.Errors.Select(e => e.Description));
                                return new SuccessResponse(
                                    StatusCodes.Status400BadRequest,
                                    $"Password validation failed: {errors}",
                                    new SuccessResponsePayload { success = false }
                                );
                            }
                        }

                        var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                        var passwordChangeResult = await _userManager.ResetPasswordAsync(identityUser, passwordResetToken, model.Password);

                        if (!passwordChangeResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            var errors = string.Join(", ", passwordChangeResult.Errors.Select(e => e.Description));
                            return new SuccessResponse(
                                StatusCodes.Status400BadRequest,
                                $"Password change failed: {errors}",
                                new SuccessResponsePayload { success = false }
                            );
                        }
                    }

                    if (!string.IsNullOrEmpty(model.Email) && model.Email != identityUser.Email)
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

                        identityUser.Email = model.Email;
                        identityUser.UserName = model.Username ?? identityUser.UserName;
                        await _userManager.UpdateAsync(identityUser);
                    }

                    subAdminUser.Username = model.Username ?? subAdminUser.Username;
                    model.IsActive = model.IsActive.HasValue ? model.IsActive.Value : subAdminUser.IsActive;
                    await _subAdminRepository.UpdateAsync(subAdminUser, true);

                    await _userPermissionRepository.DeleteByUserIdAsync(subAdminUser.UserId);

                    var newPermissions = model.Permissions.Select(pid => new UserPermission
                    {
                        IsActive = true,
                        UserId = subAdminUser.UserId,
                        PermissionId = pid
                    }).ToArray();

                    await _userPermissionRepository.InsertAsync(newPermissions, true);
                    await transaction.CommitAsync();

                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status200OK,
                        message: "Success",
                        payload: new SuccessResponsePayload { success = true }
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while creating an subdmin user");
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "An unexpected error occurred. Please try again later.",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }
            });
        }

        public async Task<GetSubAdminResponse> GetSubAdminById(Guid Id)
        {
            try
            {
                var subAdminUser = await _subAdminRepository.GetSubAdminUserByRelation(Id);
                if (subAdminUser == null)
                {
                    return new GetSubAdminResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Entity with Id {Id} not found",
                        payload: null
                    );
                }

                var payload = _mapper.Map<GetSubAdminPayload>(subAdminUser);
                var permissions = await _userPermissionRepository.GetByUserIdAsync(subAdminUser.UserId);
                payload.Permissions = permissions.Select(p => p.PermissionId).ToArray();

                return new GetSubAdminResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting subdmin user");
                return new GetSubAdminResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetSubAdminsResponse> GetSubAdmins(EntityFilter filter)
        {
            try
            {
                var query = _subAdminRepository.GetSubAdminsForFilter();
                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(admin => admin.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(admin =>
                        admin.Username.ToLower().Contains(searchKeyword) ||
                        admin.Email.ToLower().Contains(searchKeyword));
                }

                var totalItems = await query.CountAsync();
                var subAdminUsers = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new GetSubAdminsPayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                foreach (var subAdmin in subAdminUsers)
                {
                    var item = _mapper.Map<GetSubAdminPayload>(subAdmin);
                    var permissions = await _userPermissionRepository.GetByUserIdAsync(subAdmin.UserId);
                    item.Permissions = permissions.Select(p => p.PermissionId).ToArray();
                    payload.Items.Add(item);
                }

                return new GetSubAdminsResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching sub admins");
                return new GetSubAdminsResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> ToggleSubAdminStatus(ToggleStatusDTO<Guid> model)
        {
            try
            {
                var response = await _subAdminRepository.GetByIdAsync(model.Id);
                if (response == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Entity with Id {model.Id} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                response.IsActive = model.IsActive;
                await _subAdminRepository.UpdateAsync(response, true);

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

        public async Task<SuccessResponse> DeleteSubAdmin(Guid Id)
        {
            try
            {
                var response = await _subAdminRepository.GetByIdAsync(Id);
                if (response == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Entity with Id {Id} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                response.IsDelete = true;
                await _subAdminRepository.UpdateAsync(response, true);

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
    }
}
