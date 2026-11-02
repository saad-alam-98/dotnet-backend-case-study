using SaudiPlus.Request.BusinessUser;

namespace SaudiPlus.Response.BusinessUser
{
    public class BusinessCreateResponse : BaseResponse<CreateBusinessDTO>
    {
        public BusinessCreateResponse(
            int statusCode, 
            string message,
            CreateBusinessDTO? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class AdminBusinessCreateResponse : BaseResponse<BusinessInfoDetailed>
    {
        public AdminBusinessCreateResponse(
            int statusCode,
            string message,
            BusinessInfoDetailed? payload
        ) : base(statusCode, message, payload)
        {
        }
    }
}
