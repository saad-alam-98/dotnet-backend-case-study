using Newtonsoft.Json;

namespace SaudiPlus.Response.Invoice
{
    public class CreateInvoiceResponse : BaseResponse<InvoiceDetailPayload>
    {
        public CreateInvoiceResponse(
            int statusCode,
            string message,
            InvoiceDetailPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class InvoiceDetailPayload
    {
        [JsonProperty("invoiceId")]
        public required int InvoiceId; 
        
        [JsonProperty("invoiceNumber")]
        public required string InvoiceNumber;

        [JsonProperty("amount")]
        public required double Amount { get; set; }

        [JsonProperty("discount")]
        public required double Discount { get; set; }

        [JsonProperty("totalAmount")]
        public required double TotalAmount { get; set; }

        [JsonProperty("issueDate")]
        public required  DateTime IssueDate { get; set; }

        [JsonProperty("business")]
        public required InvoiceBusinessData Business { get; set; }
        
        [JsonProperty("branch")]
        public required InvoiceBranchData Branch { get; set; }

        [JsonProperty("user")]
        public required InvoiceUserData User {  get; set; }

        public void SetBusinessNameLocalized(string name)
        {
            if (Business == null)
                return;

            Business.Name = name;
        }
    }

    public class InvoiceBusinessData
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("description")]
        public required string Description { get; set; }

        [JsonProperty("mobileNumber")]
        public required string MobileNumber { get; set; }

        [JsonProperty("phoneNumber")]
        public required string PhoneNumber { get; set; }

        [JsonProperty("address")]
        public required string Address { get; set; }

        [JsonProperty("logoPath")]
        public required string LogoPath { get; set; }
    }

    public class InvoiceUserData
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("displayName")]
        public required string DisplayName { get; set; }

        [JsonProperty("phoneNumber")]
        public required string PhoneNumber { get; set; }

        [JsonProperty("address")]
        public required string Address { get; set; }

        [JsonProperty("totalPoints")]
        public required string TotalPoints { get; set; }

        [JsonProperty("avatarUrl")]
        public required string AvatarUrl { get; set; }
    }

    public class InvoiceBranchData
    {
        [JsonProperty("id")]
        public required int Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("branchNumber")]
        public required int BranchNumber { get; set; }

        [JsonProperty("address")]
        public required string Address { get; set; }
    }
}
