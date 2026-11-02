using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Request.Partner
{
    public class EditPartnerDTO
    {
        [FromForm(Name = "id")]
        public required int Id { get; set; }

        [FromForm(Name = "nameAr")]
        [MaxLength(200)] public string? NameAr { get; set; }

        [FromForm(Name = "nameEn")]
        [MaxLength(200)] public string? NameEn { get; set; }

        [FromForm(Name = "companyName")]
        [MaxLength(200)] public string? CompanyName { get; set; }

        [FromForm(Name = "isActive")]
        [MaxLength(200)] public bool? IsActive { get; set; }

        [FromForm(Name = "email")]
        [MaxLength(200)] public string? Email { get; set; }

        [FromForm(Name = "password")]
        [MaxLength(200)] public string? Password { get; set; }

        [FromForm(Name = "mobileNumber")]
        [MaxLength(200)] public string? MobileNumber { get; set; }

        [FromForm(Name = "categoryId")]
        public int? CategoryId { get; set; }

        [FromForm(Name = "logo")]
        public IFormFile? Logo { get; set; }
    }
}
