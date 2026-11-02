using SaudiPlus.Request.IndividualUser;

namespace SaudiPlus.Response.IndividualUser
{
    public class UserCreateResponse : BaseResponse<IndividualUserData>
    {
        public UserCreateResponse(
            int statusCode,
            string message,
            IndividualUserData? payload
        ) : base(statusCode, message, payload)
        {
        }
    }
}
