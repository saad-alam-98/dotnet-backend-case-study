//using TextCopy;
using System.Text;
using SaudiPlus.Enums;
using SaudiPlus.Entities;
using SaudiPlus.Constants;
using SaudiPlus.Interfaces;
using System.Security.Claims;
using SaudiPlus.Response.Account;
using SaudiPlus.Entities.Identity;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace SaudiPlus.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly SaudiPlusDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        public TokenService(
            IConfiguration configuration,
            SaudiPlusDbContext dbContext,
            UserManager<User> userManager,
            RoleManager<Role> roleManager
        )
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<TokenResponse> GenerateAccessToken(User user)
        {
            var expiry = DateTime.UtcNow.AddHours(12);
            SecurityTokenDescriptor tokenDescriptor = await GetUserTokenDescriptor(user, expiry);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            var accessToken = handler.WriteToken(token);

            var refreshToken = GenerateRefreshToken();
            await SetRefreshTokenAsync(user, refreshToken);

            user.CurrentRefreshToken = refreshToken;
            await _userManager.UpdateAsync(user);

            return new TokenResponse
            {
                accessToken = accessToken,
                refreshToken = refreshToken,
                expiry = expiry
            };
        }

  
        public async Task<(TokenResponse? response, Exception? error)> RefreshAccessTokenAsync(User user, string refreshToken)
        {
            // ✅ Allow using both Current and Previous refresh tokens for validation
            if (user.CurrentRefreshToken != refreshToken && user.PreviousRefreshToken != refreshToken)
            {
                return (null, new Exception("Invalid Refresh Token"));
            }

            var expiry = DateTime.UtcNow.AddHours(12);
            SecurityTokenDescriptor tokenDescriptor = await GetUserTokenDescriptor(user, expiry);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            var accessToken = handler.WriteToken(token);

            if (user.CurrentRefreshToken == refreshToken)
            {
                string newRefreshToken = GenerateRefreshToken();
                user.PreviousRefreshToken = user.CurrentRefreshToken; // Store old refresh token
                user.CurrentRefreshToken = newRefreshToken; // Store new refresh token
            }

            await _userManager.UpdateAsync(user); // Save in DB

            return (new TokenResponse
            {
                accessToken = accessToken,
                refreshToken = user.CurrentRefreshToken,
                expiry = expiry
            }, null);
        }

        private async Task<SecurityTokenDescriptor> GetUserTokenDescriptor(User user, DateTime expiry)
        {
            var secretKey = _configuration["Jwt:Secret"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credential = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new("AspNet.Identity.SecurityStamp", user.SecurityStamp ?? string.Empty),
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                if (Enum.TryParse<UserRole>(role, out var userRole))
                {
                    if (userRole == UserRole.SubAdmin)
                        continue;

                    if (Permissions.RolePermissions().TryGetValue(userRole, out var permissions))
                    {
                        foreach (var permission in permissions)
                        {
                            claims.Add(new Claim("permission", permission.ToString()));
                        }
                    }
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiry,
                SigningCredentials = credential,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var userPermissions = await _dbContext.UserPermissions
                .Where(p => p.UserId == user.Id)
                .ToListAsync();

            foreach (var permission in userPermissions)
            {
                tokenDescriptor.Subject.AddClaim(new Claim("permission", permission.PermissionId));
            }

            return tokenDescriptor;
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        public async Task SetRefreshTokenAsync(User user, string refreshToken)
        {
            await _userManager.SetAuthenticationTokenAsync(user, "JWTAuth", "RefreshToken", refreshToken);
        }

        public async Task<string?> GetRefreshTokenAsync(User user)
        {
            return await _userManager.GetAuthenticationTokenAsync(user, "JWTAuth", "RefreshToken");
        }

        public async Task RevokeRefreshToken(User user)
        {
            await _userManager.RemoveAuthenticationTokenAsync(user, "JWTAuth", "RefreshToken");
        }

        private byte[] GetPasswordResetEncryptionKey()
        {
            var configuredKey = _configuration["PasswordReset:EncryptionKey"]
                ?? throw new InvalidOperationException("PasswordReset:EncryptionKey must be configured.");
            var key = Convert.FromBase64String(configuredKey);
            if (key.Length is not (16 or 24 or 32))
                throw new InvalidOperationException("PasswordReset:EncryptionKey must encode a 16, 24 or 32 byte AES key.");
            return key;
        }

        public string GenerateForgetPasswordToken(string Email)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    byte[] key = GetPasswordResetEncryptionKey();
                    aes.Key = key;
                    byte[] iv = aes.IV;

                    using (MemoryStream memoryStream = new())
                    {
                        memoryStream.Write(iv, 0, iv.Length);

                        using (CryptoStream cryptoStream = new(
                            memoryStream,
                            aes.CreateEncryptor(),
                            CryptoStreamMode.Write)
                        )
                        {
                            using (StreamWriter encryptWriter = new(cryptoStream, Encoding.UTF8))
                            {
                                encryptWriter.Write(Email);
                            }
                        }


                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The encryption failed. {ex}");
                return "";
            }
        }

        public async Task<string> ValidateForgetPasswordToken(string token)
        {
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(token);

                using (MemoryStream memoryStream = new(encryptedBytes))
                {
                    using (Aes aes = Aes.Create())
                    {
                        byte[] iv = new byte[aes.IV.Length];
                        int numBytesToRead = aes.IV.Length;
                        int numBytesRead = 0;

                        while (numBytesToRead > 0)
                        {
                            int n = memoryStream.Read(iv, numBytesRead, numBytesToRead);
                            if (n == 0) break;

                            numBytesRead += n;
                            numBytesToRead -= n;
                        }

                        byte[] key = GetPasswordResetEncryptionKey();

                        using (CryptoStream cryptoStream = new(
                           memoryStream,
                           aes.CreateDecryptor(key, iv),
                           CryptoStreamMode.Read))
                        {
                            using (StreamReader decryptReader = new(cryptoStream))
                            {
                                string decryptedMessage = await decryptReader.ReadToEndAsync();
                                return decryptedMessage;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The decryption failed. {ex}");
                return "";
            }
        }

        public (string otp, bool isDefault) GenerateOtp(int length)
        {
            var otpOverride = _configuration["OtpOverride"];
            if (!string.IsNullOrEmpty(otpOverride))
            {
                return (otpOverride, true);
            }
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var otp = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
            return (otp, false);
        }
    }

    public class RevokeOldTokensService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(30);

        public RevokeOldTokensService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                    var users = userManager.Users;
                    foreach (var user in users)
                    {
                        if (!string.IsNullOrEmpty(user.PreviousRefreshToken))
                        {
                            user.PreviousRefreshToken = null; // Revoke old token
                            await userManager.UpdateAsync(user);
                        }
                    }
                }

                await Task.Delay(_interval, stoppingToken); // Wait for the next execution
            }
        }
    }
}
