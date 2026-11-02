using SaudiPlus.Response.IndividualUser;

namespace SaudiPlus.Response.User
{
    public class RegisterUserResponse : BaseResponse<IndividualUserLoginPayload>
    {
        public RegisterUserResponse(
            int statusCode,
            string message,
            IndividualUserLoginPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }
}
