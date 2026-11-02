

namespace SaudiPlus.Response.IndividualUser
{
    public class UserEditResponse : BaseResponse<IndividualUserData>
    {
        public UserEditResponse(
            int statusCode,
            string message,
            IndividualUserData? payload
        ) : base(statusCode, message, payload)
        {
        }
    }
}
