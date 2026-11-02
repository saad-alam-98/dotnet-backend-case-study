using SaudiPlus.Response;
using SaudiPlus.Request.Account;
using SaudiPlus.Response.Account;

namespace SaudiPlus.Interfaces.Service
{
    public interface IAccountService
    {
        Task<LoginResponse> Login(LoginRequestDTO model);
        Task<ForgetPasswordResponse> ForgetPasswordAsync(ForgetPasswordDTO model);
        Task<SuccessResponse> ChangePasswordAsync(ChangePasswordDTO model);
        Task<SuccessResponse> ResetPasswordAsync(ResetPasswordDTO model);
        Task<ValidateOTPResponse> ValidateOTPAsync(ValidateOtpDTO model);
        Task<SuccessResponse> LogoutUser(LogoutDTO model);
        Task<RefreshTokenResponse> RefreshAccessToken(RefreshTokenDTO model);
        Task<SuccessResponse> ConfirmPhoneRequest(Entities.IndividualUser user, RegisterPhoneNumberDTO model);
        Task<SuccessResponse> ConfirmPhoneOtp(string email, ConfirmPhoneDTO model);
    }
}
