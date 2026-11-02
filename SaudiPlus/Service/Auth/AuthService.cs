using AutoMapper;
using SaudiPlus.Enums;
using Google.Apis.Auth;
using SaudiPlus.Entities;
using SaudiPlus.Extensions;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Auth;
using SaudiPlus.Request.Auth;
using SaudiPlus.Response.User;
using SaudiPlus.Helper.Bucket;
using SaudiPlus.Entities.Identity;
using Microsoft.Extensions.Options;
using SaudiPlus.Interfaces.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using SaudiPlus.Interfaces.Repository;
using SaudiPlus.Response.IndividualUser;

namespace SaudiPlus.Service.Auth
{
    public class AuthService : BaseService<AuthService>, IAuthService
    {
        private readonly BucketSetting _bucket;
        private readonly GoogleAuth _googleAuth;
        private readonly IFileService _fileService;
        private readonly ITokenService _tokenService;
        private readonly SaudiPlusDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IIndividualUserRepository _individualUserRepository;

        public AuthService(
            IMapper mapper,
            IFileService fileService,
            ITokenService tokenService,
            ILogger<AuthService> logger,
            SaudiPlusDbContext dbContext,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptions<BucketSetting> bucket,
            IOptions<GoogleAuth> googleAuth,
            ILanguageService languageService,
            IIndividualUserRepository individualUserRepository
        ) : base(mapper, logger, languageService)
        {
            _bucket = bucket.Value;
            _dbContext = dbContext;
            _fileService = fileService;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _googleAuth = googleAuth.Value;
            _individualUserRepository = individualUserRepository;
        }

        public async Task<RegisterUserResponse> GetOrRegisterGoogleUser(GoogleAuthRequestDTO model)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    GoogleJsonWebSignature.Payload data;
                    
                    try 
                    {
                        // First attempt to validate using standard GoogleJsonWebSignature
                        data = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, new GoogleJsonWebSignature.ValidationSettings
                        {
                            Audience = new[] { _googleAuth.ClientId }
                        });
                    }
                    catch (Exception validationEx)
                    {
                        // Alternate approach using HttpClient
                        try 
                        {
                            using var client = new HttpClient();
                            var response = await client.GetAsync($"https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={model.IdToken}");
                            
                            if (!response.IsSuccessStatusCode)
                            {
                                await transaction.RollbackAsync();
                                return new RegisterUserResponse(
                                    StatusCodes.Status400BadRequest, 
                                    "Invalid Google token. Please try again with a valid token.",
                                    null
                                );
                            }
                            
                            var tokenInfo = await response.Content.ReadFromJsonAsync<GoogleUserTokenInfo>();
                            if (tokenInfo?.Email == null)
                            {
                                await transaction.RollbackAsync();
                                return new RegisterUserResponse(
                                    StatusCodes.Status400BadRequest,
                                    "Google token does not contain required email information",
                                    null
                                );
                            }
                            
                            // Create a payload object to match standard validation
                            data = new GoogleJsonWebSignature.Payload
                            {
                                Email = tokenInfo.Email,
                                Name = tokenInfo.Name ?? "Google User",
                                Picture = tokenInfo.Picture
                            };
                        }
                        catch (Exception httpEx)
                        {
                            _logger.LogError(httpEx, "Alternative Google token validation failed");
                            await transaction.RollbackAsync();
                            return new RegisterUserResponse(
                                StatusCodes.Status400BadRequest,
                                "Invalid Google token format",
                                null
                            );
                        }
                    }

                    if (string.IsNullOrEmpty(data.Name) || string.IsNullOrEmpty(data.Picture))
                    {
                        using var client = new HttpClient();
                        var response = await client.GetAsync($"https://www.googleapis.com/oauth2/v3/userinfo?access_token={model.IdToken}");

                        if (response.IsSuccessStatusCode)
                        {
                            var userInfo = await response.Content.ReadFromJsonAsync<GoogleUserInfo>();
                            data.Name = userInfo?.Name ?? "Google User";
                            data.Picture = userInfo?.Picture;
                        }
                    }

                    var user = await _userManager.FindByEmailAsync(data.Email);
                    if (user == null)
                    {
                        user = new User
                        {
                            Email = data.Email,
                            UserName = $"{data.Email.Split('@')[0]}_{Guid.NewGuid():N}".Substring(0, 10),
                            IsActive = true
                        };

                        var createResult = await _userManager.CreateAsync(user);
                        if (!createResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return new RegisterUserResponse(StatusCodes.Status500InternalServerError,
                                string.Join(", ", createResult.Errors.Select(e => e.Description)), null);
                        }

                        await _roleManager.EnsureRoleExistsAsync(UserRole.User.ToString(), user.Id);
                        await _userManager.AddUserToRole(user, UserRole.User.ToString());
                    }

                    var IndividualUser = await _individualUserRepository.GetUserWithRelationsByEmail(data.Email);
                    if (IndividualUser == null)
                    {
                        IndividualUser = new Entities.IndividualUser
                        {
                            Email = data.Email,
                            DisplayName = data.Name,
                            AvatarUrl = data.Picture,
                            UserId = user.Id,
                            IsActive = true,
                            IsDelete = false,
                            LoginType = LoginType.Google.ToString()
                        };
                        await _individualUserRepository.InsertAsync(IndividualUser, true);
                    }

                    var token = await _tokenService.GenerateAccessToken(user);
                    await transaction.CommitAsync();

                    return new RegisterUserResponse(
                        StatusCodes.Status200OK,
                        "User Registered",
                        new IndividualUserLoginPayload
                        {
                            AccessToken = token.accessToken,
                            RefreshToken = token.refreshToken,
                            DeviceToken = IndividualUser.DeviceToken ?? "",
                            IsPhoneConfirmed = user.PhoneNumberConfirmed,
                            Expiry = token.expiry,
                        }
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error in Google Sign-In");
                    return new RegisterUserResponse(
                        StatusCodes.Status500InternalServerError,
                        "An error occurred",
                        null
                    );
                }
            });
        }

        public async Task<RegisterUserResponse> GetOrRegisterGoogleAppleUser(AppleAuthRequestDTO model)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(model.IdToken) as JwtSecurityToken;

                    var email = jsonToken?.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                    var userId = jsonToken?.Subject;

                    if (string.IsNullOrEmpty(email))
                    {
                        email = $"{userId}@privaterelay.appleid.com";
                    }

                    var fullName = string.IsNullOrEmpty(model.FullName) ? "Apple User" : model.FullName;

                    var user = await _userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        user = new User
                        {
                            Email = email,
                            UserName = $"{email.Split('@')[0]}_{Guid.NewGuid():N}".Substring(0, 10),
                            IsActive = true
                        };

                        var createResult = await _userManager.CreateAsync(user);
                        if (!createResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return new RegisterUserResponse(StatusCodes.Status500InternalServerError,
                                string.Join(", ", createResult.Errors.Select(e => e.Description)), null);
                        }

                        await _roleManager.EnsureRoleExistsAsync(UserRole.User.ToString(), user.Id);
                        await _userManager.AddUserToRole(user, UserRole.User.ToString());
                    }

                    var IndividualUser = await _individualUserRepository.GetUserWithRelationsByEmail(email);
                    if (IndividualUser == null)
                    {
                        IndividualUser = new Entities.IndividualUser
                        {
                            Email = email,
                            DisplayName = fullName,
                            UserId = user.Id,
                            IsActive = true,
                            IsDelete = false,
                            LoginType = LoginType.Apple.ToString()
                        };
                        await _individualUserRepository.InsertAsync(IndividualUser, true);
                    }

                    var token = await _tokenService.GenerateAccessToken(user);
                    await transaction.CommitAsync();

                    return new RegisterUserResponse(
                        StatusCodes.Status200OK,
                        "User Registered",
                        new IndividualUserLoginPayload
                        {
                            AccessToken = token.accessToken,
                            RefreshToken = token.refreshToken,
                            DeviceToken = IndividualUser.DeviceToken ?? "",
                            IsPhoneConfirmed = user.PhoneNumberConfirmed,
                            Expiry = token.expiry,
                        }
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error in Apple Sign-In");
                    return new RegisterUserResponse(
                        StatusCodes.Status500InternalServerError,
                        "An error occurred",
                        null
                    );
                }
            });
        }
    }

    public class GoogleUserInfo
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
    }

    public class GoogleUserTokenInfo
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
        
        [JsonPropertyName("aud")]
        public string? Audience { get; set; }
        
        [JsonPropertyName("exp")]
        public string? ExpirationTime { get; set; }
    }
}
