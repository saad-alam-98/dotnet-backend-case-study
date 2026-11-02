using Newtonsoft.Json;

namespace SaudiPlus.Response.BusinessRequest
{
    public class BusinessRequestResponse : BaseResponse<BusinessRequestPayload>
    {
        public BusinessRequestResponse(
            int statusCode,
            string message,
            BusinessRequestPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class BusinessRequestPayload
    {
        [JsonProperty("firstName")]
        public required string FirstName { get; set; }

        [JsonProperty("lastName")]
        public required string LastName { get; set; }

        [JsonProperty("email")]
        public required string Email { get; set; }

        [JsonProperty("phoneNumber")]
        public required string PhoneNumber { get; set; }

        [JsonProperty("businessName")]
        public required string BusinessName { get; set; }

        [JsonProperty("categoryId")]
        public required string CategoryId { get; set; }

        [JsonProperty("isActive")]
        public required bool IsActive { get; set; }
    }
}
