using AutoMapper;
using SaudiPlus.Enums;
using SaudiPlus.Entities;
using SaudiPlus.Response;
using SaudiPlus.Interfaces;
using SaudiPlus.Request.Account;
using SaudiPlus.Response.Account;
using SaudiPlus.Entities.Identity;
using SaudiPlus.Interfaces.Service;
using Microsoft.AspNetCore.Identity;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Service.Account
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        protected readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly ILanguageService _languageService;
        private readonly ISubAdminRepository _subAdminRepository;
        private readonly ITextMessageService _textMessageService;
        private readonly IBusinessRepository _businessUserRepository;
        private readonly IForgetPasswordRepository _forgetPasswordRepository;
        private readonly IIndividualUserRepository _individualUserRepository;

        public AccountService(
            IMapper mapper,
            UserManager<User> userManager,
            IEmailService emailService,
            ITokenService tokenService,
            ILanguageService languageService,
            ISubAdminRepository subAdminRepository,
            ITextMessageService textMessageService,
            IBusinessRepository businessUserRepository,
            IForgetPasswordRepository forgetPasswordRepository,
            IIndividualUserRepository individualUserRepository
        )
        {
            _mapper = mapper;
            _userManager = userManager;
            _emailService = emailService;
            _tokenService = tokenService;
            _languageService = languageService;
            _subAdminRepository = subAdminRepository;
            _textMessageService = textMessageService;
            _businessUserRepository = businessUserRepository;
            _forgetPasswordRepository = forgetPasswordRepository;
            _individualUserRepository = individualUserRepository;
        }

        public async Task<LoginResponse> Login(LoginRequestDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                var error = _userManager.ErrorDescriber.InvalidEmail(model.Email);
                return new LoginResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: error.Description,
                    payload: null
                );
            }

            var isValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isValid)
            {
                var error = _userManager.ErrorDescriber.PasswordMismatch();
                return new LoginResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: error.Description,
                    payload: null
                );
            }

            try
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, UserRole.Admin.ToString());
                if (isAdmin)
                {
                    var token = await _tokenService.GenerateAccessToken(user);
                    var responseData = _mapper.Map<LoginData>(user);
                    responseData.DisplayName = user.UserName!;
                    return new LoginResponse(
                        statusCode: StatusCodes.Status200OK,
                        message: "Login Success",
                        payload: new LoginResponsePayload
                        {
                            accessToken = token.accessToken,
                            refreshToken = token.refreshToken,
                            expiry = token.expiry,
                            role = UserRole.Admin.ToString(),
                            data = responseData
                        }
                    );
                }
                var isBusiness = await _userManager.IsInRoleAsync(user, UserRole.Business.ToString());
                if (isBusiness)
                {
                    var token = await _tokenService.GenerateAccessToken(user);
                    var business = await _businessUserRepository.GetBusinessWithRelation(user.Email);
                    var payload = new LoginData()
                    {
                        Id = business.Id,
                        Email = business.Email,
                        DisplayName = business.CompanyName
                    };

                    return new LoginResponse(
                        statusCode: StatusCodes.Status200OK,
                        message: "Login Success",
                        payload: new LoginResponsePayload
                        {
                            accessToken = token.accessToken,
                            refreshToken = token.refreshToken,
                            expiry = token.expiry,
                            role = UserRole.Business.ToString(),
                            data = payload
                        }
                    );
                }
                var isSubAdmin = await _userManager.IsInRoleAsync(user, UserRole.SubAdmin.ToString());
                if (isSubAdmin)
                {
                    var token = await _tokenService.GenerateAccessToken(user);
                    var subAdmin = await _subAdminRepository.GetSubAdminUserByUserId(user.Id);
                    var payload = new LoginData()
                    {
                        Id = subAdmin.Id,
                        Email = subAdmin.Email,
                        DisplayName = subAdmin.Username
                    };

                    return new LoginResponse(
                        statusCode: StatusCodes.Status200OK,
                        message: "Login Success",
                        payload: new LoginResponsePayload
                        {
                            accessToken = token.accessToken,
                            refreshToken = token.refreshToken,
                            expiry = token.expiry,
                            role = UserRole.SubAdmin.ToString(),
                            data = payload
                        }
                    );
                }

                return new LoginResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: _userManager.ErrorDescriber.DefaultError().Description,
                    payload: null
                );
            }
            catch (Exception ex)
            {
                return new LoginResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        public async Task<ForgetPasswordResponse> ForgetPasswordAsync(ForgetPasswordDTO model)
        {
            var (languageCode, error) = _languageService.GetLanguageCode();
            if (error != null)
            {
                return new ForgetPasswordResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: error.Message,
                    payload: null
                );
            }

            var email = model.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new ForgetPasswordResponse(
                    StatusCodes.Status400BadRequest,
                    _userManager.ErrorDescriber.InvalidEmail(model.Email).Description,
                    null
                );
            }

            var existingRequest = await _forgetPasswordRepository.FindByUserIdAsync(user.Id);

            if (existingRequest != null)
            {
                await _forgetPasswordRepository.DeleteAsync(existingRequest.Id, true);
            }

            var otp = _tokenService.GenerateOtp(4);
            string accessToken = _tokenService.GenerateForgetPasswordToken(model.Email);
            DateTime expiry = DateTime.UtcNow.AddMinutes(2);

            var forgetPassword = new ForgetPassword
            {
                UserId = user.Id,
                Email = model.Email,
                Otp = otp.otp,
                ExpiryTime = expiry,
                AccessToken = accessToken
            };

            try
            {
                var payload = _mapper.Map<ForgetPasswordPayload>(forgetPassword);
                await _forgetPasswordRepository.InsertAsync(forgetPassword, true);

                if (!otp.isDefault)
                {
                    string subject = languageCode == Language.ar ?
                        "رمز إعادة تعيين كلمة المرور" : "Your Saudi Plus Password Reset Code";

                    var placeholders = new Dictionary<string, string>
                    {
                        { "OTP", otp.otp }
                    };

                    string body = _emailService.LoadEmailTemplate("PasswordReset", languageCode, placeholders);
                    var (Success, Message) = await _emailService.SendEmailAsync(subject, body, model.Email);
                }

                return new ForgetPasswordResponse
                (
                    StatusCodes.Status200OK,
                    "Email Sent",
                    payload
                );
            }
            catch (Exception ex)
            {
                return new ForgetPasswordResponse
                (
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<ValidateOTPResponse> ValidateOTPAsync(ValidateOtpDTO model)
        {
            var email = await _tokenService.ValidateForgetPasswordToken(model.AccessToken);
            if (email == "")
            {
                return new ValidateOTPResponse(
                   StatusCodes.Status400BadRequest,
                   "Invalid Token",
                   null
               );
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new ValidateOTPResponse(
                    StatusCodes.Status400BadRequest,
                    _userManager.ErrorDescriber.InvalidUserName(email).Description,
                    null
                );
            }

            var request = await _forgetPasswordRepository.FindByUserIdAsync(user.Id);

            if (request == null)
            {
                return new ValidateOTPResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid or expired request.",
                    null
                );
            }

            if (request.ExpiryTime < DateTime.UtcNow)
            {
                await _forgetPasswordRepository.DeleteAsync(request.Id, true);

                return new ValidateOTPResponse(
                    StatusCodes.Status400BadRequest,
                    "OTP expired. Please request a new one.",
                    null
                );
            }

            if (model.Otp != request.Otp)
            {
                return new ValidateOTPResponse
                (
                    StatusCodes.Status400BadRequest,
                    "InValid OTP",
                    null
                );
            }

            try
            {
                string accessToken = _tokenService.GenerateForgetPasswordToken(email);
                await _forgetPasswordRepository.DeleteAsync(request.Id, true);
                return new ValidateOTPResponse
                (
                    StatusCodes.Status200OK,
                    "OTP Verified",
                    new ValidateOTPPayload
                    {
                        isVerified = true,
                        accessToken = accessToken
                    }
                );
            }
            catch (Exception ex)
            {
                return new ValidateOTPResponse
                (
                    StatusCodes.Status400BadRequest,
                    ex.Message,
                    null
                );
            }
        }

        public async Task<SuccessResponse> ChangePasswordAsync(ChangePasswordDTO model)
        {
            var email = await _tokenService.ValidateForgetPasswordToken(model.AccessToken);
            if (string.IsNullOrEmpty(email))
            {
                return new SuccessResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Token",
                    new SuccessResponsePayload { success = false }
                );
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new SuccessResponse(
                    StatusCodes.Status400BadRequest,
                    _userManager.ErrorDescriber.InvalidUserName(email).Description,
                    new SuccessResponsePayload { success = false }
                );
            }

            if (model.Password != model.ConfirmPassword)
            {
                return new SuccessResponse(
                    StatusCodes.Status400BadRequest,
                    "Passwords do not match.",
                    new SuccessResponsePayload { success = false }
                );
            }

            foreach (var validator in _userManager.PasswordValidators)
            {
                var validationResult = await validator.ValidateAsync(_userManager, user, model.Password);
                if (!validationResult.Succeeded)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.Description));
                    return new SuccessResponse(
                        StatusCodes.Status400BadRequest,
                        $"Password validation failed: {errors}",
                        new SuccessResponsePayload { success = false }
                    );
                }
            }

            try
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new SuccessResponse(
                        StatusCodes.Status400BadRequest,
                        $"Password reset failed: {errors}",
                        new SuccessResponsePayload { success = false }
                    );
                }

                return new SuccessResponse(
                    StatusCodes.Status200OK,
                    "Password successfully changed.",
                    new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse(
                    StatusCodes.Status500InternalServerError,
                    $"An error occurred: {ex.Message}",
                    new SuccessResponsePayload { success = false }
                );
            }
        }


        public async Task<SuccessResponse> LogoutUser(LogoutDTO model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return new SuccessResponse(
                        StatusCodes.Status400BadRequest,
                        _userManager.ErrorDescriber.InvalidUserName(model.Email).Description,
                        new SuccessResponsePayload { success = false }
                    );
                }

                await _tokenService.RevokeRefreshToken(user);

                return new SuccessResponse
                (
                    StatusCodes.Status200OK,
                    "Logout Success",
                    new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse
                (
                    StatusCodes.Status400BadRequest,
                    ex.Message,
                    new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<RefreshTokenResponse> RefreshAccessToken(RefreshTokenDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                var error = _userManager.ErrorDescriber.InvalidEmail(model.Email);
                return new RefreshTokenResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: error.Description,
                    payload: null
                );
            }

            var token = await _tokenService.RefreshAccessTokenAsync(user, model.RefreshToken);

            if (token.error != null)
            {
                return new RefreshTokenResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: token.error.Message,
                    payload: null
                );
            }

            return new RefreshTokenResponse(
                statusCode: StatusCodes.Status500InternalServerError,
                message: "New Token Generated",
                payload: new TokenResponse
                {
                    accessToken = token.response.accessToken,
                    refreshToken = token.response.refreshToken,
                    expiry = token.response.expiry,
                }
            );
        }

        public async Task<SuccessResponse> ResetPasswordAsync(ResetPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                var error = _userManager.ErrorDescriber.InvalidEmail(model.Email);
                return new SuccessResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: error.Description,
                    new SuccessResponsePayload { success = false }
                );
            }

            if (model.Password != model.ConfirmPassword)
            {
                return new SuccessResponse(
                    StatusCodes.Status400BadRequest,
                     _userManager.ErrorDescriber.PasswordMismatch().Description,
                    new SuccessResponsePayload { success = false }
                );
            }

            foreach (var validator in _userManager.PasswordValidators)
            {
                var validationResult = await validator.ValidateAsync(_userManager, user, model.Password);
                if (!validationResult.Succeeded)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.Description));
                    return new SuccessResponse(
                        StatusCodes.Status400BadRequest,
                        $"Password validation failed: {errors}",
                        new SuccessResponsePayload { success = false }
                    );
                }
            }

            try
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new SuccessResponse(
                        StatusCodes.Status400BadRequest,
                        $"Password reset failed: {errors}",
                        new SuccessResponsePayload { success = false }
                    );
                }

                return new SuccessResponse(
                    StatusCodes.Status200OK,
                    "Password successfully changed.",
                    new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse(
                    StatusCodes.Status500InternalServerError,
                    $"An error occurred: {ex.Message}",
                    new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> ConfirmPhoneRequest(Entities.IndividualUser user, RegisterPhoneNumberDTO model)
        {
            var identity = await _userManager.FindByIdAsync(user.UserId.ToString());

            if (identity == null)
            {
                return new SuccessResponse(
                    StatusCodes.Status400BadRequest,
                    "User not fount",
                     new SuccessResponsePayload { success = false }
                );
            }

            try
            {
                var otp = _tokenService.GenerateOtp(4);
                identity.NumberConfimationToken = otp.otp;
                identity.NumberConfimationExpiry = DateTime.UtcNow.AddMinutes(2);
                identity.PhoneNumber = model.PhoneNumber;
                user.PhoneNumber = model.PhoneNumber;

                await _userManager.UpdateAsync(identity);
                await _individualUserRepository.UpdateAsync(user, true);

                if (!otp.isDefault)
                {
                    var message = $"Saudi Plus OTP: {otp.otp}. This code expires in 2 minutes. Do NOT share it with anyone.";
                    var (Success, Exception) = await _textMessageService.SendSms(user.PhoneNumber, message);
                }

                return new SuccessResponse(
                    StatusCodes.Status200OK,
                    "Otp Sent",
                    new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse
                (
                    StatusCodes.Status400BadRequest,
                    "An unexpected error occurred. Please try again later.",
                     new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> ConfirmPhoneOtp(string email, ConfirmPhoneDTO model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return new SuccessResponse(
                        StatusCodes.Status400BadRequest,
                        _userManager.ErrorDescriber.InvalidUserName(email).Description,
                         new SuccessResponsePayload { success = false }
                    );
                }

                if (user.NumberConfimationToken != model.Otp ||
                    user.NumberConfimationExpiry < DateTime.UtcNow
                )
                {
                    return new SuccessResponse(
                        StatusCodes.Status400BadRequest,
                        "Invalid OTP or expired token.",
                        new SuccessResponsePayload { success = true }
                    );
                }

                user.PhoneNumberConfirmed = true;
                user.NumberConfimationToken = null;
                user.NumberConfimationExpiry = null;

                await _userManager.UpdateAsync(user);

                return new SuccessResponse(
                    StatusCodes.Status200OK,
                    "Phone Number Confirmed",
                    new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse
                (
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                     new SuccessResponsePayload { success = false }
                );
            }
        }
    }
}
