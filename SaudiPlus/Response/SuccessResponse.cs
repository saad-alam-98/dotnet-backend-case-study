namespace SaudiPlus.Response
{
    public class SuccessResponse : BaseResponse<SuccessResponsePayload>
    {
        public SuccessResponse(
            int statusCode, 
            string message,
            SuccessResponsePayload payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class SuccessResponsePayload
    {
        public required bool success;
    }
}
