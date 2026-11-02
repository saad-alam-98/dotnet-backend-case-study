using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Request.BusinessUser
{
    public class EditBusinessDTO
    {
        [FromForm(Name = "nameAr")][MaxLength(50)] public string? NameAr { get; set; }
        [FromForm(Name = "nameEn")][MaxLength(50)] public string? NameEn { get; set; }
        [FromForm(Name = "password")] public string? Password { get; set; }
        [FromForm(Name = "email")] public string? Email { get; set; }
        [FromForm(Name = "isActive")] public bool? IsActive { get; set; }
        [FromForm(Name = "mobileNumber")] public string? MobileNumber { get; set; }
        [FromForm(Name = "phoneNumber")] public string? PhoneNumber { get; set; }
        [FromForm(Name = "address")][MaxLength(256)] public string? Address { get; set; }
        [FromForm(Name = "description")][MaxLength(50)] public string? Description { get; set; }
        [FromForm(Name = "categoryId")] public int? categoryId { get; set; }
        [FromForm(Name = "logo")] public IFormFile? Logo { get; set; }
    }

    public class EditBusinessByAdmin : EditBusinessDTO
    {
        [FromForm(Name = "businessId")] public required Guid BusinessId { get; set; }
    }
}
