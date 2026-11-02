using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Request.Banner
{
    public class EditBannerDTO
    {
        [FromForm(Name = "id")]
        public required int Id { get; set; }

        [FromForm(Name = "title")]
        [MaxLength(200)] public string? Title { get; set; }

        [FromForm(Name = "isActive")]
        public bool? IsActive { get; set; }

        [FromForm(Name = "businessId")]
        public Guid? BusinessId { get; set; }

        [FromForm(Name = "categoryId")]
        public int? CategoryId { get; set; }

        [FromForm(Name = "cityId")]
        public int? CityId { get; set; }

        [FromForm(Name = "Image")]
        public IFormFile? image { get; set; }
    }
}
