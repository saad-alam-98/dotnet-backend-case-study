using Microsoft.AspNetCore.Mvc;

namespace SaudiPlus.Request.Category
{
    public class EditCategoryDTO
    {
        [FromForm(Name = "id")]
        public required int Id { get; set; }

        [FromForm(Name = "nameAr")]
        public string? NameAr { get; set; }

        [FromForm(Name = "NameEn")]
        public string? NameEn { get; set; }

        [FromForm(Name = "isActive")]
        public bool? IsActive { get; set; }

        [FromForm(Name = "icon")]
        public IFormFile? icon { get; set; }
    }
}
