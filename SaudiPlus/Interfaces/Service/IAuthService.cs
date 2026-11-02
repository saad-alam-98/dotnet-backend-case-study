using SaudiPlus.Request.Auth;
using SaudiPlus.Response.User;

namespace SaudiPlus.Interfaces.Service
{
    public interface IAuthService
    {
        Task<RegisterUserResponse> GetOrRegisterGoogleUser(GoogleAuthRequestDTO model);
        Task<RegisterUserResponse> GetOrRegisterGoogleAppleUser(AppleAuthRequestDTO model);
    }
}
