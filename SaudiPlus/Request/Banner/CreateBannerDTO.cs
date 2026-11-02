using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Request.Banner
{
    public class CreateBannerDTO
    {
        [FromForm(Name = "title")]
        [MaxLength(200)] public required string Title { get; set; }

        [FromForm(Name = "isActive")]
        public required bool isActive { get; set; }

        [FromForm(Name = "businessId")]
        public Guid? BusinessId { get; set; }

        [FromForm(Name = "categoryId")]
        public int? CategoryId { get; set; }

        [FromForm(Name = "cityId")]
        public required int CityId { get; set; }

        [FromForm(Name = "banner")]
        public required IFormFile banner { get; set; }
    }
}
