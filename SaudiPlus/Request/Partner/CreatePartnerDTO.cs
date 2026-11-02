using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Request.Partner
{
    public class CreatePartnerDTO
    {
        [FromForm(Name = "nameAr")]
        [MaxLength(200)] public required string NameAr { get; set; }

        [FromForm(Name = "nameEn")]
        [MaxLength(200)] public required string NameEn { get; set; }

        [FromForm(Name = "companyName")]
        [MaxLength(200)] public required string CompanyName { get; set; }

        [FromForm(Name = "email")]
        [MaxLength(200)] public required string Email { get; set; }

        [FromForm(Name = "password")]
        [MaxLength(200)] public required string Password { get; set; }

        [FromForm(Name = "mobileNumber")]
        [MaxLength(200)] public required string MobileNumber { get; set; }

        [JsonProperty("categoryId")]
        public required int CategoryId { get; set; }

        [FromForm(Name = "logo")]
        public required IFormFile Logo { get; set; }
    }
}
