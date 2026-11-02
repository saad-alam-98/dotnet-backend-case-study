using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Request.BusinessUser
{
    public class AdminCreateBusinessDTO
    {
        [FromForm(Name = "email")][MaxLength(50)] public required string Email { get; set; }
        [FromForm(Name = "nameAr")][MaxLength(50)] public required string NameAr { get; set; }
        [FromForm(Name = "nameEn")][MaxLength(50)] public required string NameEn { get; set; }
        [FromForm(Name = "password")] public required string Password { get; set; }
        [FromForm(Name = "mobileNumber")] public required string MobileNumber { get; set; }
        [FromForm(Name = "phoneNumber")] public required string PhoneNumber { get; set; }
        [FromForm(Name = "address")][MaxLength(256)] public string? Address { get; set; }
        [FromForm(Name = "description")][MaxLength(512)] public string? Description { get; set; }
        [FromForm(Name = "categoryId")] public int categoryId { get; set; }
        [FromForm(Name = "logo")] public required IFormFile Logo { get; set; }
    }
}
