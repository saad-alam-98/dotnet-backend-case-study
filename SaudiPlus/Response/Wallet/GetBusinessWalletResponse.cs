using Newtonsoft.Json;

namespace SaudiPlus.Response.Wallet
{
    public class GetBusinessWalletResponse : BaseResponse<BusinessWalletPayload>
    {
        public GetBusinessWalletResponse(
            int statusCode,
            string message,
            BusinessWalletPayload? payload
        ) : base(statusCode, message, payload)
        {
        }
    }

    public class BusinessWalletPayload
    {
        [JsonProperty("sales")]
        public required BusinessWalletSalesItem Sales { get; set; }

        [JsonProperty("invoice")]
        public required BusinessInvoiceItem Invoice { get; set; }
    }

    public class BusinessWalletSalesItem
    {
        [JsonProperty("totalSales")]
        public required double TotalSales { get; set; }

        [JsonProperty("items")]
        public required List<StoreSalesItem> Items;

        [JsonProperty("totalItems")]
        public required int TotalItems { get; set; }

        [JsonProperty("hasNextPage")]
        public required bool HasNextPage { get; set; }
    }

    public class StoreSalesItem
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("branch")]
        public required BranchSalesItem Branch { get; set; }

        [JsonProperty("price")]
        public required double Price { get; set; }

        [JsonProperty("discount")]
        public required double Discount { get; set; }

        [JsonProperty("finalAmount")]
        public required double FinalAmount { get; set; }

        [JsonProperty("issueDate")]
        public required DateTime IssueDate { get; set; }
    }

    public class BranchSalesItem
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("address")]
        public required string Address { get; set; }
    }

    public class BusinessInvoiceItem
    {
        [JsonProperty("invoiceAmount")]
        public required double InvoiceAmount { get; set; }

        [JsonProperty("name")]
        public required string InvoiceNumber { get; set; }

        [JsonProperty("issueDate")]
        public required string IssueDate { get; set; }

        [JsonProperty("mobileNumber")]
        public required string MobileNumber { get; set; }

        [JsonProperty("nameEn")]
        public required string NameEn { get; set; }

        [JsonProperty("nameAr")]
        public required string NameAr { get; set; }

        [JsonProperty("details")]
        public required InvoiceDetails Details { get; set; }
    }

    public class InvoiceDetails
    {
        [JsonProperty("description")]
        public required string Description { get; set; }

        [JsonProperty("quantity")]
        public required int Quantity { get; set; }

        [JsonProperty("sales")]
        public required double Sales { get; set; }

        [JsonProperty("unitCost")]
        public required float UnitCost { get; set; }
    }
}
