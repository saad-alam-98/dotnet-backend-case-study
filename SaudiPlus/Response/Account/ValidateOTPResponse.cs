namespace SaudiPlus.Response.Account
{
    public class ValidateOTPResponse : BaseResponse<ValidateOTPPayload>
    {
        public ValidateOTPResponse(
            int statusCode, 
            string message, 
            ValidateOTPPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class ValidateOTPPayload
    {
        public required bool isVerified;
        public required string accessToken;
    }
}
