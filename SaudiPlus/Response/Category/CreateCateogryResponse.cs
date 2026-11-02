namespace SaudiPlus.Response.Category
{
    public class CreateCateogryResponse : BaseResponse<CategoryInfoDetailed>
    {
        public CreateCateogryResponse(
            int statusCode, 
            string message, 
            CategoryInfoDetailed? payload
        ) : base(statusCode, message, payload)
        {
        }
    }
}
