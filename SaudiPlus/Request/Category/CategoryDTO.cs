using Microsoft.AspNetCore.Mvc;

namespace SaudiPlus.Request.Category
{
    public class CategoryDTO
    {
        [FromForm(Name = "nameAr")]
        public required string NameAr { get; set; }
        [FromForm(Name = "nameEn")]
        public required string NameEn { get; set; }
        [FromForm(Name = "icon")]
        public required IFormFile icon { get; set; }
    }
}
