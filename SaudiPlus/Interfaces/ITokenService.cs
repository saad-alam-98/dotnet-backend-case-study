using SaudiPlus.Entities.Identity;
using SaudiPlus.Response.Account;

namespace SaudiPlus.Interfaces
{
    public interface ITokenService
    {
        public Task<TokenResponse> GenerateAccessToken(User user);
        public string GenerateForgetPasswordToken(string Email);
        public Task<string> ValidateForgetPasswordToken(string token);
        public Task SetRefreshTokenAsync(User user, string refreshToken);
        public Task<string?> GetRefreshTokenAsync(User user);
        public Task RevokeRefreshToken(User user);
        Task<(TokenResponse? response, Exception? error)> RefreshAccessTokenAsync(User user, string refreshToken);
        public (string otp, bool isDefault) GenerateOtp(int length);
    }
}
