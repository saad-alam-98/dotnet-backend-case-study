using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Request.Status
{
    public class CreateStatusDTO
    {
        [FromForm(Name = "title")]
        [MaxLength(50)]
        public required string Title { get; set; }

        [FromForm(Name = "media")]
        public required IFormFile Media { get; set; }

        [FromForm(Name = "duration")]
        public double? Duration { get; set; }

        [FromForm(Name = "description")]
        [MaxLength(50)]
        public string? Description { get; set; }

        [FromForm(Name = "linkUrl")]
        public string? LinkUrl { get; set; }

        [FromForm(Name = "offerId")]
        public int? OfferId { get; set; }
    }
}
