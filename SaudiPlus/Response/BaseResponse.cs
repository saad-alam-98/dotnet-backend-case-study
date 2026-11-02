using Microsoft.AspNetCore.Mvc;

namespace SaudiPlus.Response
{
    public class BaseResponse
    {
        public int statusCode { get; set; }
        public string message { get; set; }

        public BaseResponse(int statusCode, string message)
        {
            this.statusCode = statusCode;
            this.message = message;
        }
    }
    public class BaseResponse<T> : BaseResponse
    {
        public T? payload { get; set; }

        public BaseResponse(int statusCode, string message, T? payload)
            : base(statusCode, message)
        {
            this.payload = payload;
        }
    }
}
