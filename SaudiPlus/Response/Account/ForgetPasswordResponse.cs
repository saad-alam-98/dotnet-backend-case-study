namespace SaudiPlus.Response.Account
{
    public class ForgetPasswordResponse : BaseResponse<ForgetPasswordPayload>
    {
        public ForgetPasswordResponse(
            int statusCode,
            string message,
            ForgetPasswordPayload? payload
        )
            : base(statusCode, message, payload)
        {
        }
    }

    public class ForgetPasswordPayload
    {
        public required string accessToken { get; set; }
    }
}
